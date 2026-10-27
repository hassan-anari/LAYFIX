using System.ComponentModel;
using System.Threading.Channels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using Layfix.Core;
using Layfix.Windows;

namespace Layfix.App;

public sealed class LayfixApplication : Application
{
    private IClassicDesktopStyleApplicationLifetime? desktop;
    private ThemeManager? theme;
    private WindowsNotifications? notifications;
    private LanguageStateService? language;
    private LanguageObservationWorker? observation;
    private AppViewModel? model;
    private WidgetWindow? widget;
    private SettingsWindow? settings;
    private TrayIcon? tray;
    private SettingsStore? store;
    private readonly Channel<AppSettings> saves = Channel.CreateBounded<AppSettings>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });
    private Task? saveWorker;
    private Task? switching;
    private EventWaitHandle? activation;
    private RegisteredWaitHandle? activationWait;
    private bool exiting;
    public override void Initialize() => theme = new ThemeManager(this);
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            desktop = lifetime;
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            desktop.ShutdownRequested += OnShutdownRequested;
            Dispatcher.UIThread.Post(Start);
        }
        base.OnFrameworkInitializationCompleted();
    }
    private async void Start()
    {
        try
        {
            store = new SettingsStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LAYFIX", "settings.json"));
            var loaded = await Task.Run(() => store.LoadAsync());
            if (exiting) return;
            model = new AppViewModel(loaded.Settings);
            model.SetNotice(loaded.Notice);
            notifications = new WindowsNotifications();
            await notifications.Ready;
            if (exiting) return;
            language = new LanguageStateService(new WindowsLanguagePlatform(new Win32InputApi(notifications)));
            language.SetPaused(model.Paused);
            notifications.SetPaused(model.Paused);
            language.Changed += OnLanguage;
            language.SwitchChanged += OnSwitch;
            observation = new LanguageObservationWorker(language);
            notifications.Changed += OnNativeChange;
            notifications.AppearanceChanged += OnAppearance;
            theme!.Apply(model.Settings);
            theme.Changed += OnTokens;
            model.SuggestedAccent = theme.Tokens.Accent.ToString();
            widget = new WidgetWindow(model, theme, Switch);
            model.SettingsChanged += OnSettings;
            saveWorker = Task.Run(async () =>
            {
                await foreach (var value in saves.Reader.ReadAllAsync())
                {
                    var result = await store.SaveAsync(value).ConfigureAwait(false);
                    Dispatcher.UIThread.Post(() => model.SetNotice(result));
                }
            });
            tray = new TrayIcon { Icon = LoadIcon(), ToolTipText = "LAYFIX", IsVisible = true };
            tray.Clicked += OnTrayClick;
            TrayIcon.SetIcons(this, new TrayIcons { tray });
            UpdateTray();
            observation.Start();
            ApplyVisibility();
            await RefreshLayouts();
            if (desktop?.Args?.Contains("--smoke-test") == true)
            {
                bool healthy = widget.FocusProtectionReady && notifications.ActiveHookCount == 3 && notifications.SessionNotificationsAvailable;
                await Stop(healthy ? 0 : 2);
                return;
            }
            if (exiting) return;
            activation = new EventWaitHandle(false, EventResetMode.AutoReset, @"Local\LAYFIX.ShowSettings");
            activationWait = ThreadPool.RegisterWaitForSingleObject(activation, (_, _) => Dispatcher.UIThread.Post(OpenSettings), null, Timeout.Infinite, false);
        }
        catch (Win32Exception)
        {
            // A tray/settings-only failure must not leave an invisible resident process.
            await Stop(1);
        }
    }
    internal static WindowIcon LoadIcon() => new(AssetLoader.Open(new Uri("avares://Layfix.App/Assets/layfix.ico")));
    private void OnTokens(AppearanceTokens tokens) { if (model != null) model.SuggestedAccent = tokens.Accent.ToString(); }
    private void OnLanguage(LanguageSnapshot value) => Dispatcher.UIThread.Post(() => model?.Observe(value));
    private void OnSwitch(SwitchOutcome value) => Dispatcher.UIThread.Post(() => model?.SetOutcome(value));
    private void OnNativeChange() => observation?.Signal();
    private void OnAppearance() => Dispatcher.UIThread.Post(() => { if (model != null) theme?.Apply(model.Settings); widget?.ApplyPosition(); });
    private void OnSettings(AppSettings value)
    {
        saves.Writer.TryWrite(value);
        language?.SetPaused(value.Paused);
        notifications?.SetPaused(value.Paused);
        observation?.Signal();
        theme?.Apply(value);
        ApplyVisibility();
        UpdateTray();
    }
    private void ApplyVisibility()
    {
        if (widget == null || model == null) return;
        if (model.WidgetEnabled && model.WidgetVisible)
        {
            widget.Show();
            if (!widget.FocusProtectionReady) widget.Hide();
        }
        else widget.Hide();
    }
    private void Switch(LanguageSnapshot captured)
    {
        if (language == null || model == null || exiting) return;
        var preferences = model.Settings;
        switching = Task.Run(() => language.SwitchAsync(captured, preferences.EnglishLayoutId, preferences.PersianLayoutId));
    }
    private void OnTrayClick(object? sender, EventArgs e) => OpenSettings();
    private void OpenSettings()
    {
        if (model == null || theme == null || exiting) return;
        if (settings == null)
        {
            settings = new SettingsWindow(model, theme, RefreshFromSettings);
            settings.Closed += (_, _) => settings = null;
        }
        settings.Show(); settings.Activate();
    }
    private async void RefreshFromSettings() { await RefreshLayouts(); settings?.RefreshKeyboardChoices(); }
    private async Task RefreshLayouts()
    {
        if (language == null || notifications == null) return;
        var layouts = await Task.Run(() => new Win32InputApi(notifications).GetLayouts());
        model?.SetLayouts(layouts);
    }
    private void UpdateTray()
    {
        if (tray == null || model == null) return;
        var menu = new NativeMenu();
        void Add(string key, Action action)
        {
            var item = new NativeMenuItem(model.S(key)); item.Click += (_, _) => action(); menu.Items.Add(item);
        }
        Add("Settings", OpenSettings);
        Add(model.WidgetEnabled && model.WidgetVisible ? "HideWidget" : "ShowWidget", () => model.Update(s => s with { WidgetEnabled = true, WidgetVisible = !(s.WidgetEnabled && s.WidgetVisible) }));
        Add(model.Paused ? "Resume" : "Pause", () => model.Paused = !model.Paused);
        menu.Items.Add(new NativeMenuItemSeparator());
        Add("Exit", Exit);
        tray.Menu = menu;
    }
    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        if (exiting) return;
        e.Cancel = true; Exit();
    }
    private async void Exit() => await Stop();
    private async Task Stop(int exitCode = 0)
    {
        if (exiting) return;
        exiting = true;
        activationWait?.Unregister(null); activation?.Dispose();
        if (model != null) model.SettingsChanged -= OnSettings;
        if (notifications != null)
        {
            notifications.Changed -= OnNativeChange;
            notifications.AppearanceChanged -= OnAppearance;
            notifications.SetPaused(true);
        }
        language?.SetPaused(true);
        if (observation != null) await observation.DisposeAsync();
        if (language != null)
        {
            await language.DisposeAsync();
        }
        if (switching != null) await switching;
        if (language != null) { language.Changed -= OnLanguage; language.SwitchChanged -= OnSwitch; }
        notifications?.Dispose();
        saves.Writer.TryComplete();
        if (saveWorker != null) await saveWorker;
        settings?.Close(); widget?.Close();
        if (tray != null) { tray.Clicked -= OnTrayClick; tray.IsVisible = false; tray.Dispose(); }
        if (theme != null) { theme.Changed -= OnTokens; theme.Dispose(); }
        if (desktop != null) { desktop.ShutdownRequested -= OnShutdownRequested; desktop.Shutdown(exitCode); }
    }
}
