using System.ComponentModel;
using System.Runtime.CompilerServices;
using Layfix.Core;

namespace Layfix.App;

public sealed class AppViewModel(AppSettings settings) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<AppSettings>? SettingsChanged;
    public AppSettings Settings { get; private set; } = settings;
    public LanguageSnapshot Snapshot { get; private set; } = LanguageSnapshot.Unavailable;
    public SwitchOutcome Outcome { get; private set; }
    public SettingsNotice Notice { get; private set; }
    public IReadOnlyList<KeyboardLayout> Layouts { get; private set; } = [];
    public string S(string key) => Strings.Get(key, Settings.Locale);
    public string StatusText => S(Snapshot.Status == ObservationStatus.Available ? "Available." + Snapshot.Layout?.Language : Snapshot.Status.ToString());
    public string OutcomeText => S("Switch." + Outcome);
    public bool FocusProtectionFailed { get; private set; }
    public string WidgetNotice => FocusProtectionFailed ? S("NonActivateFailed") : "";
    public void SetFocusProtectionFailure(bool value) { FocusProtectionFailed = value; RaiseAll(); }
    public string NoticeText => S("Notice." + Notice);
    public string LayoutText => Layouts.Count == 0 ? S("Unavailable") : string.Join(Environment.NewLine, Layouts.Select(x => x.Name + (x.Language == InputLanguage.Unsupported ? " — " + S("Unsupported") : "")));
    public void Observe(LanguageSnapshot value) { Snapshot = value; RaiseAll(); }
    public void SetOutcome(SwitchOutcome value) { Outcome = value; RaiseAll(); }
    public void SetNotice(SettingsNotice value) { Notice = value; RaiseAll(); }
    public void SetLayouts(IReadOnlyList<KeyboardLayout> value) { Layouts = value; RaiseAll(); }
    public void Update(Func<AppSettings, AppSettings> change)
    {
        var next = change(Settings).Validate();
        if (next == Settings) return;
        Settings = next;
        RaiseAll();
        SettingsChanged?.Invoke(next);
    }
    public bool WidgetEnabled { get => Settings.WidgetEnabled; set => Update(s => s with { WidgetEnabled = value }); }
    public bool WidgetVisible { get => Settings.WidgetVisible; set => Update(s => s with { WidgetVisible = value }); }
    public bool Paused { get => Settings.Paused; set => Update(s => s with { Paused = value }); }
    public bool DragLocked { get => Settings.DragLocked; set => Update(s => s with { DragLocked = value }); }
    public bool AlwaysOnTop { get => Settings.AlwaysOnTop; set => Update(s => s with { AlwaysOnTop = value }); }
    public double Opacity { get => Settings.Opacity; set => Update(s => s with { Opacity = value }); }
    public double Scale { get => Settings.Scale; set => Update(s => s with { Scale = value }); }
    public int ThemeIndex { get => (int)Settings.Theme; set => Update(s => s with { Theme = (ThemeMode)value }); }
    public int GlassIndex { get => (int)Settings.Glass; set => Update(s => s with { Glass = (GlassPreference)value }); }
    public int LocaleIndex { get => Settings.Locale == "fa" ? 1 : 0; set => Update(s => s with { Locale = value == 1 ? "fa" : "en" }); }
    public string SuggestedAccent { get; set; } = "#0078D4";
    public bool FollowAccent { get => Settings.Accent == null; set => Update(s => s with { Accent = value ? null : SuggestedAccent }); }
    public void RaiseAll() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
}
