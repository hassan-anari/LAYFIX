using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Animation;
using Layfix.Core;
using Layfix.Windows;

namespace Layfix.App;

public sealed class WidgetWindow : Window
{
    private readonly AppViewModel model;
    private readonly ThemeManager theme;
    private readonly Action<LanguageSnapshot> switchLanguage;
    private readonly Border english;
    private readonly Border persian;
    private readonly TextBlock englishText;
    private readonly TextBlock persianText;
    private readonly TextBlock marker;
    private PixelPoint pressed;
    private PixelPoint startPosition;
    private LanguageSnapshot? captured;
    private IPointer? pointer;
    private bool dragging;
    private bool configuring;
    public bool FocusProtectionReady { get; private set; }
    public WidgetWindow(AppViewModel viewModel, ThemeManager themeManager, Action<LanguageSnapshot> switchAction)
    {
        model = viewModel; theme = themeManager; switchLanguage = switchAction;
        Title = "LAYFIX";
        SystemDecorations = SystemDecorations.None;
        ShowInTaskbar = false;
        ShowActivated = false;
        CanResize = false;
        Width = 148; Height = 46;
        FontFamily = new FontFamily("Segoe UI, Tahoma");
        Win32Properties.AddWindowStylesCallback(this, WindowStyles);
        Win32Properties.AddWndProcHookCallback(this, WindowMessage);
        englishText = Label("EN"); persianText = Label("FA"); marker = Label("?");
        marker.FontSize = 13;
        english = Segment(englishText); persian = Segment(persianText);
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,22,*"), Margin = new Thickness(5) };
        grid.Children.Add(english);
        Grid.SetColumn(marker, 1); grid.Children.Add(marker);
        Grid.SetColumn(persian, 2); grid.Children.Add(persian);
        Content = grid;
        AutomationProperties.SetName(this, model.S("SwitchAction"));
        Cursor = new Cursor(StandardCursorType.Hand);
        PointerPressed += OnPressed;
        PointerMoved += OnMoved;
        PointerReleased += OnReleased;
        PointerCaptureLost += (_, _) => { pointer = null; captured = null; dragging = false; };
        Opened += (_, _) =>
        {
            var handle = TryGetPlatformHandle()?.Handle ?? 0;
            FocusProtectionReady = handle != 0 && WindowsAppearance.ConfigureNonActivating(handle);
            model.SetFocusProtectionFailure(!FocusProtectionReady);
            if (!FocusProtectionReady) { Hide(); return; }
            Apply();
        };
        PropertyChanged += (_, e) =>
        {
            if (e.Property == ActualTransparencyLevelProperty) ThemeManager.Surface(this, theme.Tokens, true);

        };
        ScalingChanged += OnScreensChanged;
        Screens.Changed += OnScreensChanged;
        model.PropertyChanged += OnModelChanged;
        theme.Changed += OnThemeChanged;
        Closed += (_, _) =>
        {
            ScalingChanged -= OnScreensChanged;
            Screens.Changed -= OnScreensChanged;
            model.PropertyChanged -= OnModelChanged;
            theme.Changed -= OnThemeChanged;
            Win32Properties.RemoveWindowStylesCallback(this, WindowStyles);
            Win32Properties.RemoveWndProcHookCallback(this, WindowMessage);
        };
        Apply();
    }
    private static (uint, uint) WindowStyles(uint style, uint exStyle) => (style, exStyle | 0x08000000 | 0x80);
    private static nint WindowMessage(nint hwnd, uint message, nint wparam, nint lparam, ref bool handled)
    {
        if (message == 0x0021) { handled = true; return 3; } // MA_NOACTIVATE: deliver the click, preserve keyboard focus.
        return 0;
    }
    private static TextBlock Label(string text) => new() { Text = text, FontSize = 15, FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
    private static Border Segment(Control content) => new() { Child = content, CornerRadius = new CornerRadius(18) };
    private void OnScreensChanged(object? sender, EventArgs e) => ApplyPosition();
    private void OnModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => Apply();
    private void OnThemeChanged(AppearanceTokens tokens) => Apply();
    public void Apply()
    {
        if (configuring) return;
        configuring = true;
        try
        {
            var tokens = theme.Tokens;
            Topmost = model.AlwaysOnTop;
            Width = 148 * model.Scale; Height = 46 * model.Scale;
            englishText.FontSize = persianText.FontSize = 15 * model.Scale;
            // Surface opacity is configurable; labels and active segments remain fully opaque.
            Opacity = 1;
            ThemeManager.Surface(this, tokens, true);
            bool available = model.Snapshot.Status == ObservationStatus.Available;
            bool en = available && model.Snapshot.Layout?.Language == InputLanguage.English;
            bool fa = available && model.Snapshot.Layout?.Language == InputLanguage.Persian;
            english.Background = en ? ThemeManager.Brush(tokens.Accent) : Brushes.Transparent;
            persian.Background = fa ? ThemeManager.Brush(tokens.Accent) : Brushes.Transparent;
            englishText.Foreground = ThemeManager.Brush(en ? tokens.AccentText : tokens.Text);
            persianText.Foreground = ThemeManager.Brush(fa ? tokens.AccentText : tokens.Text);
            marker.Foreground = ThemeManager.Brush(tokens.Text);
            marker.Text = model.Outcome == SwitchOutcome.Pending ? "…" : model.Outcome is not (SwitchOutcome.None or SwitchOutcome.Succeeded) ? "!" : available ? "" : model.Snapshot.Indicator;
            ToolTip.SetTip(this, model.StatusText + Environment.NewLine + model.OutcomeText + Environment.NewLine + model.S("DragHelp"));
            AutomationProperties.SetName(this, model.S("SwitchAction") + ": " + model.StatusText);
            foreach (var segment in new[] { english, persian })
                segment.Transitions = tokens.Animate ? new Transitions { new BrushTransition { Property = Border.BackgroundProperty, Duration = TimeSpan.FromMilliseconds(120) } } : null;
            if (!dragging) ApplyPosition();
        }
        finally { configuring = false; }
    }
    private IReadOnlyList<MonitorBounds> Monitors() => Screens.All.Select(s => new MonitorBounds(s.WorkingArea.X, s.WorkingArea.Y, s.WorkingArea.Width, s.WorkingArea.Height, s.Scaling)).ToArray();
    public void ApplyPosition()
    {
        var position = WidgetPosition.Clamp(dragging ? new(Position.X, Position.Y) : model.Settings.Position, Monitors(), Width, Height);
        Position = new PixelPoint(position.X, position.Y);
        nint handle = TryGetPlatformHandle()?.Handle ?? 0;
        if (handle != 0) WindowsAppearance.ClipPill(handle, (int)Math.Ceiling(Width * RenderScaling), (int)Math.Ceiling(Height * RenderScaling));
    }
    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        pressed = this.PointToScreen(e.GetPosition(this)); startPosition = Position;
        captured = model.Snapshot; pointer = e.Pointer; dragging = false;
        pointer.Capture(this); e.Handled = true;
    }
    private void OnMoved(object? sender, PointerEventArgs e)
    {
        if (pointer != e.Pointer || captured == null) return;
        var now = this.PointToScreen(e.GetPosition(this));
        int dx = now.X - pressed.X, dy = now.Y - pressed.Y;
        if (dx * dx + dy * dy > 36 * RenderScaling * RenderScaling) dragging = true;
        if (dragging && !model.DragLocked)
        {
            var p = WidgetPosition.Clamp(new(startPosition.X + dx, startPosition.Y + dy), Monitors(), Width, Height);
            Position = new PixelPoint(p.X, p.Y);
        }
    }
    private void OnReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (pointer != e.Pointer || captured == null) return;
        var original = captured;
        bool moved = dragging;
        pointer = null; captured = null; dragging = false;
        e.Pointer.Capture(null);
        if (moved && !model.DragLocked) model.Update(s => s with { Position = new(Position.X, Position.Y) });
        else if (!moved && FocusProtectionReady) switchLanguage(original);
        e.Handled = true;
    }
}
