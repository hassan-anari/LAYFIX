using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Layfix.Core;

namespace Layfix.App;

public sealed class SettingsWindow : Window
{
    private readonly AppViewModel model;
    private readonly ThemeManager theme;
    private readonly Action refreshLayouts;
    private string locale;
    private TabControl? tabs;
    public SettingsWindow(AppViewModel viewModel, ThemeManager themeManager, Action refresh)
    {
        model = viewModel; theme = themeManager; refreshLayouts = refresh; locale = model.Settings.Locale;
        Width = 800; Height = 640; MinWidth = 640; MinHeight = 480;
        FontFamily = new FontFamily("Segoe UI, Tahoma"); FontSize = 14;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Icon = LayfixApplication.LoadIcon();
        DataContext = model;
        model.PropertyChanged += OnModelChanged;
        theme.Changed += OnTheme;
        PropertyChanged += (_, e) => { if (e.Property == ActualTransparencyLevelProperty) OnTheme(theme.Tokens); };
        Closed += (_, _) => { model.PropertyChanged -= OnModelChanged; theme.Changed -= OnTheme; };
        Build(); OnTheme(theme.Tokens);
    }
    private void OnTheme(AppearanceTokens tokens) => ThemeManager.Surface(this, tokens, false);
    private void OnModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (locale != model.Settings.Locale) { locale = model.Settings.Locale; Build(); }
    }
    private void Build()
    {
        int selected = tabs?.SelectedIndex ?? 0;
        Title = "LAYFIX · " + model.S("Settings");
        FlowDirection = locale == "fa" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        var root = new DockPanel { Margin = new Thickness(24) };
        var header = new StackPanel { Spacing = 4, Margin = new Thickness(0, 0, 0, 20) };
        header.Children.Add(new TextBlock { Text = "LAYFIX", FontSize = 27, FontWeight = FontWeight.SemiBold, FlowDirection = FlowDirection.LeftToRight });
        var notice = Text("", 13); notice.Bind(TextBlock.TextProperty, new Binding(nameof(AppViewModel.NoticeText)));
        header.Children.Add(notice);
        DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        tabs = new TabControl { TabStripPlacement = Dock.Left, SelectedIndex = selected, ItemsSource = new[]
        {
            Tab("General", General()), Tab("Appearance", Appearance()), Tab("Widget", Widget()), Tab("Keyboard", Keyboard()), Tab("About", About())
        } };
        root.Children.Add(tabs); Content = root;
    }
    private TabItem Tab(string key, Control content) => new() { Header = model.S(key), Content = new ScrollViewer { Content = content, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Margin = locale == "fa" ? new Thickness(0, 0, 24, 0) : new Thickness(24, 0, 0, 0) } };
    private StackPanel Page(string title) => new() { Spacing = 16, Children = { new TextBlock { Text = model.S(title), FontSize = 22, FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 0, 0, 8) } } };
    private static TextBlock Text(string text, double size = 14) => new() { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap, LineHeight = 22 };
    private CheckBox Check(string key, string property)
    {
        var control = new CheckBox { Content = model.S(key), MinHeight = 36 };
        control.Bind(ToggleButton.IsCheckedProperty, new Binding(property) { Mode = BindingMode.TwoWay });
        return control;
    }
    private Button Action(string key, Action action)
    {
        var control = new Button { Content = model.S(key), MinHeight = 36, HorizontalAlignment = HorizontalAlignment.Left };
        control.Click += (_, _) => action();
        return control;
    }
    private Control Choice(string key, string[] options, string property)
    {
        var panel = new StackPanel { Spacing = 6 };
        panel.Children.Add(Text(model.S(key)));
        var control = new ComboBox { ItemsSource = options, HorizontalAlignment = HorizontalAlignment.Stretch, MinHeight = 36 };
        AutomationProperties.SetName(control, model.S(key));
        control.Bind(SelectingItemsControl.SelectedIndexProperty, new Binding(property) { Mode = BindingMode.TwoWay });
        panel.Children.Add(control); return panel;
    }
    private Control Range(string key, string property, double minimum, double maximum, double step)
    {
        var panel = new StackPanel { Spacing = 4 };
        var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        row.Children.Add(Text(model.S(key)));
        var number = Text(""); number.Bind(TextBlock.TextProperty, new Binding(property) { StringFormat = "{0:P0}" });
        Grid.SetColumn(number, 1); row.Children.Add(number); panel.Children.Add(row);
        var slider = new Slider { Minimum = minimum, Maximum = maximum, TickFrequency = step, IsSnapToTickEnabled = true, MinHeight = 36 };
        AutomationProperties.SetName(slider, model.S(key));
        slider.Bind(RangeBase.ValueProperty, new Binding(property) { Mode = BindingMode.TwoWay });
        panel.Children.Add(slider); return panel;
    }
    private Control General()
    {
        var panel = Page("General");
        panel.Children.Add(Check("EnableWidget", nameof(AppViewModel.WidgetEnabled)));
        panel.Children.Add(Check("ShowWidget", nameof(AppViewModel.WidgetVisible)));
        panel.Children.Add(Check("Pause", nameof(AppViewModel.Paused)));
        panel.Children.Add(Choice("UILanguage", ["English", "فارسی"], nameof(AppViewModel.LocaleIndex)));
        var widgetNotice = Text(""); widgetNotice.Bind(TextBlock.TextProperty, new Binding(nameof(AppViewModel.WidgetNotice))); panel.Children.Add(widgetNotice);
        panel.Children.Add(Text(model.S("ResidentHelp")));
        panel.Children.Add(Action("RestoreDefaults", () => { model.Update(_ => new AppSettings()); Build(); }));
        return panel;
    }
    private Control Appearance()
    {
        var panel = Page("Appearance");
        panel.Children.Add(Choice("Theme", [model.S("System"), model.S("Light"), model.S("Dark")], nameof(AppViewModel.ThemeIndex)));
        panel.Children.Add(Check("FollowAccent", nameof(AppViewModel.FollowAccent)));
        var colorPanel = new StackPanel { Spacing = 6 };
        colorPanel.Children.Add(Text(model.S("CustomAccent")));
        var picker = new ColorPicker { Color = Color.Parse(model.Settings.Accent ?? theme.Tokens.Accent.ToString()), IsAlphaEnabled = false, IsAlphaVisible = false, MinHeight = 36, HorizontalAlignment = HorizontalAlignment.Left };
        AutomationProperties.SetName(picker, model.S("CustomAccent"));
        picker.ColorChanged += (_, e) => model.Update(s => s with { Accent = new Rgb(e.NewColor.R, e.NewColor.G, e.NewColor.B).ToString() });
        colorPanel.Children.Add(picker);
        panel.Children.Add(colorPanel);
        panel.Children.Add(Action("RestoreColors", () => { model.Update(s => s with { Accent = null, Theme = ThemeMode.System }); Build(); }));
        panel.Children.Add(Range("Opacity", nameof(AppViewModel.Opacity), .65, 1, .05));
        panel.Children.Add(Range("Size", nameof(AppViewModel.Scale), .8, 1.8, .1));
        panel.Children.Add(Choice("Glass", [model.S("System"), model.S("Opaque")], nameof(AppViewModel.GlassIndex)));
        panel.Children.Add(Text(model.S("GlassHelp")));
        return panel;
    }
    private Control Widget()
    {
        var panel = Page("Widget");
        panel.Children.Add(Check("LockDrag", nameof(AppViewModel.DragLocked)));
        panel.Children.Add(Check("Topmost", nameof(AppViewModel.AlwaysOnTop)));
        panel.Children.Add(Action("ResetPosition", () => model.Update(s => s with { Position = null, WidgetVisible = true, WidgetEnabled = true })));
        panel.Children.Add(Text(model.S("DragHelp")));
        return panel;
    }
    private Control Keyboard()
    {
        var panel = Page("Keyboard");
        panel.Children.Add(Text(model.S("ActiveLanguage")));
        var status = Text(""); status.FontWeight = FontWeight.SemiBold;
        status.Bind(TextBlock.TextProperty, new Binding(nameof(AppViewModel.StatusText))); panel.Children.Add(status);
        panel.Children.Add(Text(model.S("LastSwitch")));
        var outcome = Text(""); outcome.Bind(TextBlock.TextProperty, new Binding(nameof(AppViewModel.OutcomeText))); panel.Children.Add(outcome);
        panel.Children.Add(Text(model.S("InstalledLayouts")));
        var layouts = Text(""); layouts.FlowDirection = FlowDirection.LeftToRight;
        layouts.Bind(TextBlock.TextProperty, new Binding(nameof(AppViewModel.LayoutText))); panel.Children.Add(layouts);
        panel.Children.Add(LayoutChoice("EnglishTarget", InputLanguage.English, model.Settings.EnglishLayoutId));
        panel.Children.Add(LayoutChoice("PersianTarget", InputLanguage.Persian, model.Settings.PersianLayoutId));
        panel.Children.Add(Action("RefreshLayouts", refreshLayouts));
        panel.Children.Add(Text(model.S("KeyboardHelp")));
        return panel;
    }
    private sealed record LayoutOption(string? Id, string Label) { public override string ToString() => Label; }
    private Control LayoutChoice(string key, InputLanguage language, string? preferred)
    {
        var panel = new StackPanel { Spacing = 6 };
        panel.Children.Add(Text(model.S(key)));
        var options = new List<LayoutOption> { new(null, model.S("Automatic")) };
        options.AddRange(model.Layouts.Where(x => x.Language == language).Select(x => new LayoutOption(x.Id, x.Name)));
        if (preferred != null && options.All(x => x.Id != preferred)) options.Add(new(preferred, preferred + " — " + model.S("Unavailable")));
        var combo = new ComboBox { ItemsSource = options, SelectedItem = options.First(x => x.Id == preferred), HorizontalAlignment = HorizontalAlignment.Stretch, MinHeight = 36 };
        AutomationProperties.SetName(combo, model.S(key));
        combo.SelectionChanged += (_, _) =>
        {
            if (combo.SelectedItem is LayoutOption option) model.Update(s => language == InputLanguage.English ? s with { EnglishLayoutId = option.Id } : s with { PersianLayoutId = option.Id });
        };
        panel.Children.Add(combo); return panel;
    }
    public void RefreshKeyboardChoices() => Build();
    internal async Task CaptureSmokeViews(string directory)
    {
        Directory.CreateDirectory(directory);
        for (int index = 0; index < 5; index++)
        {
            tabs!.SelectedIndex = index;
            await Dispatcher.UIThread.InvokeAsync(UpdateLayout, DispatcherPriority.Loaded);
            using var bitmap = new RenderTargetBitmap(new PixelSize(800, 640), new Vector(96, 96));
            bitmap.Render(this);
            bitmap.Save(Path.Combine(directory, $"settings-{locale}-{index}.png"));
        }
    }
    private Control About()
    {
        var panel = Page("About");
        panel.Children.Add(Text("LAYFIX " + (Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.1.0-preview")));
        panel.Children.Add(Text(model.S("Preview")));
        panel.Children.Add(Text(model.S("Privacy")));
        panel.Children.Add(Action("Repository", () => Process.Start(new ProcessStartInfo("https://github.com/hassan-anari/LAYFIX") { UseShellExecute = true })));
        return panel;
    }
}
