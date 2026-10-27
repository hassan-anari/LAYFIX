using System.Globalization;

namespace Layfix.Core;

public readonly record struct Rgb(byte R, byte G, byte B)
{
    public override string ToString() => $"#{R:X2}{G:X2}{B:X2}";
    public static bool TryParse(string? value, out Rgb color)
    {
        color = default;
        if (value is not { Length: 7 } || value[0] != '#' || !uint.TryParse(value.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint rgb)) return false;
        color = new((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        return true;
    }
    public double Luminance
    {
        get
        {
            static double Linear(byte c) => c / 255d <= .04045 ? c / 255d / 12.92 : Math.Pow((c / 255d + .055) / 1.055, 2.4);
            return .2126 * Linear(R) + .7152 * Linear(G) + .0722 * Linear(B);
        }
    }
    public static double Contrast(Rgb a, Rgb b) => (Math.Max(a.Luminance, b.Luminance) + .05) / (Math.Min(a.Luminance, b.Luminance) + .05);
}
public sealed record SystemAppearance(bool Dark, Rgb Accent, bool HighContrast, bool Transparency, bool Animations, Rgb? SystemSurface = null, Rgb? SystemText = null, Rgb? SystemHighlightText = null);
public sealed record AppearanceTokens(bool Dark, Rgb Accent, Rgb AccentText, Rgb Surface, Rgb Text, Rgb Secondary, Rgb Outline, bool Glass, double Opacity, bool Animate)
{
    // Accent used as a thin line/glyph must remain visible against the surface.
    public Rgb AccentOnSurface
    {
        get
        {
            if (Rgb.Contrast(Accent, Surface) >= 3) return Accent;
            for (int step = 1; step <= 20; step++)
            {
                double amount = step / 20d;
                byte Mix(byte from, byte to) => (byte)Math.Round(from + (to - from) * amount);
                var candidate = new Rgb(Mix(Accent.R, Text.R), Mix(Accent.G, Text.G), Mix(Accent.B, Text.B));
                if (Rgb.Contrast(candidate, Surface) >= 3) return candidate;
            }
            return Text;
        }
    }
    public static AppearanceTokens Create(AppSettings settings, SystemAppearance system)
    {
        var black = new Rgb(0, 0, 0);
        var white = new Rgb(255, 255, 255);
        bool dark = settings.Theme == ThemeMode.Dark || (settings.Theme == ThemeMode.System && system.Dark);
        var accent = !system.HighContrast && Rgb.TryParse(settings.Accent, out var custom) ? custom : system.Accent;
        var accentText = Rgb.Contrast(accent, black) >= Rgb.Contrast(accent, white) ? black : white;
        var surface = system.HighContrast ? system.SystemSurface ?? black : dark ? new Rgb(32, 32, 32) : new Rgb(249, 249, 249);
        var text = system.HighContrast ? system.SystemText ?? white : dark ? new Rgb(255, 255, 255) : new Rgb(26, 26, 26);
        if (system.HighContrast) accentText = system.SystemHighlightText ?? accentText;
        return new(dark, accent, accentText, surface, text,
            system.HighContrast ? text : dark ? new Rgb(205, 205, 205) : new Rgb(85, 85, 85),
            system.HighContrast ? text : dark ? new Rgb(112, 112, 112) : new Rgb(130, 130, 130),
            !system.HighContrast && system.Transparency && settings.Glass == GlassPreference.System,
            !system.HighContrast && system.Transparency && settings.Glass == GlassPreference.System ? settings.Opacity : 1, !system.HighContrast && system.Animations);
    }
}
