using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Layfix.Core;
using Layfix.Windows;

namespace Layfix.App;

public sealed class ThemeManager : IDisposable
{
    private readonly Application app;
    private readonly FluentTheme fluent;
    private readonly IPlatformSettings? platform;
    private AppSettings settings = new();
    public event Action<AppearanceTokens>? Changed;
    public AppearanceTokens Tokens { get; private set; } = AppearanceTokens.Create(new(), new(false, new(0, 120, 212), false, false, false));
    public ThemeManager(Application application)
    {
        app = application;
        fluent = new FluentTheme();
        app.Styles.Add(fluent);
        app.Styles.Add(new StyleInclude(new Uri("avares://Layfix.App/")) { Source = new Uri("avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml") });
        platform = app.PlatformSettings;
        if (platform != null) platform.ColorValuesChanged += OnColors;
    }
    private void OnColors(object? sender, PlatformColorValues values) => Avalonia.Threading.Dispatcher.UIThread.Post(() => Apply(settings));
    public void Apply(AppSettings value)
    {
        settings = value;
        var colors = platform?.GetColorValues();
        var accent = colors?.AccentColor1 ?? Avalonia.Media.Color.FromRgb(0, 120, 212);
        // Avalonia's platform accent is authoritative. DWM is the supported native fallback.
        var system = WindowsAppearance.Read(colors?.ThemeVariant == PlatformThemeVariant.Dark, new(accent.R, accent.G, accent.B));
        if (colors != null && !system.HighContrast) system = system with { Accent = new(accent.R, accent.G, accent.B) };
        Tokens = AppearanceTokens.Create(settings, system);
        app.RequestedThemeVariant = Tokens.Dark ? ThemeVariant.Dark : ThemeVariant.Light;
        foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            var palette = new ColorPaletteResources { Accent = Color(Tokens.Accent) };
            if (system.HighContrast)
            {
                palette.BaseHigh = Color(Tokens.Text);
                palette.BaseMedium = Color(Tokens.Text);
                palette.BaseMediumHigh = Color(Tokens.Text);
                palette.BaseMediumLow = Color(Tokens.Text);
                palette.AltHigh = Color(Tokens.Surface);
                palette.RegionColor = Color(Tokens.Surface);
                palette.ChromeLow = Color(Tokens.Surface);
                palette.ChromeMedium = Color(Tokens.Surface);
            }
            fluent.Palettes[variant] = palette;
        }
        app.Resources["LayfixSurface"] = Brush(Tokens.Surface);
        app.Resources["LayfixText"] = Brush(Tokens.Text);
        app.Resources["LayfixSecondary"] = Brush(Tokens.Secondary);
        app.Resources["LayfixAccent"] = Brush(Tokens.Accent);
        app.Resources["LayfixAccentText"] = Brush(Tokens.AccentText);
        Changed?.Invoke(Tokens);
    }
    public static Color Color(Rgb value) => Avalonia.Media.Color.FromRgb(value.R, value.G, value.B);
    public static SolidColorBrush Brush(Rgb value) => new(Color(value));
    public static void Surface(Window window, AppearanceTokens tokens, bool widget)
    {
        window.TransparencyLevelHint = tokens.Glass
            ? widget ? [WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur, WindowTransparencyLevel.None]
                : [WindowTransparencyLevel.Mica, WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.None]
            : [WindowTransparencyLevel.None];
        bool backdrop = window.ActualTransparencyLevel != WindowTransparencyLevel.None;
        window.Background = tokens.Glass && backdrop ? new SolidColorBrush(Color(tokens.Surface), widget ? tokens.Opacity : .96) : Brush(tokens.Surface);
        window.Foreground = Brush(tokens.Text);
        window.TransparencyBackgroundFallback = Brush(tokens.Surface);
    }
    public void Dispose()
    {
        if (platform != null) platform.ColorValuesChanged -= OnColors;
        Changed = null;
    }
}
