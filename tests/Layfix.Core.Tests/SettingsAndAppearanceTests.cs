using Layfix.Core;

namespace Layfix.Core.Tests;

public sealed class SettingsAndAppearanceTests
{
    [Fact]
    public void ValidationRepairsInvalidValues()
    {
        var settings = new AppSettings { Theme = (ThemeMode)99, Glass = (GlassPreference)99, Opacity = double.NaN, Scale = double.PositiveInfinity, Accent = "bad", Locale = "invalid", EnglishLayoutId = "../path", Position = new(int.MaxValue, 0) }.Validate();
        Assert.Equal(new AppSettings(), settings);
        var clamped = new AppSettings { Opacity = .1, Scale = 5, Accent = "#abcdef", Locale = "fa" }.Validate();
        Assert.Equal(.65, clamped.Opacity);
        Assert.Equal(1.8, clamped.Scale);
        Assert.Equal("#ABCDEF", clamped.Accent);
        Assert.Equal("fa", clamped.Locale);
    }
    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{\"Opacity\":\"oops\"}")]
    public async Task CorruptSettingsRecoverWithoutStartupFailure(string content)
    {
        string directory = Temp();
        try
        {
            string path = Path.Combine(directory, "settings.json");
            await File.WriteAllTextAsync(path, content);
            var result = await new SettingsStore(path).LoadAsync();
            Assert.Equal(new AppSettings(), result.Settings);
            Assert.Equal(SettingsNotice.Recovered, result.Notice);
        }
        finally { Directory.Delete(directory, true); }
    }
    [Fact]
    public async Task AtomicRoundTripAndConcurrentWritesLeaveValidFile()
    {
        string directory = Temp();
        try
        {
            string path = Path.Combine(directory, "settings.json");
            var store = new SettingsStore(path);
            Assert.Equal(new AppSettings(), (await store.LoadAsync()).Settings);
            var expected = new AppSettings { Locale = "fa", Accent = "#123456", Position = new(-300, 400), DragLocked = true };
            Assert.Equal(SettingsNotice.None, await store.SaveAsync(expected));
            Assert.Equal(expected, (await store.LoadAsync()).Settings);
            var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(i => store.SaveAsync(expected with { Scale = 1 + i / 20d })));
            Assert.All(results, value => Assert.Equal(SettingsNotice.None, value));
            Assert.InRange((await store.LoadAsync()).Settings.Scale, 1, 1.55);
            Assert.Single(Directory.GetFiles(directory));
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => store.SaveAsync(new(), cancelled.Token));
            Assert.Equal("fa", (await store.LoadAsync()).Settings.Locale);
        }
        finally { Directory.Delete(directory, true); }
    }
    [Fact]
    public async Task FutureSchemaIsNeverOverwritten()
    {
        string directory = Temp();
        try
        {
            string path = Path.Combine(directory, "settings.json");
            string original = "{\"SchemaVersion\":2}";
            await File.WriteAllTextAsync(path, original);
            var store = new SettingsStore(path);
            Assert.Equal(SettingsNotice.NewerVersion, (await store.LoadAsync()).Notice);
            Assert.Equal(SettingsNotice.NewerVersion, await store.SaveAsync(new()));
            Assert.Equal(original, await File.ReadAllTextAsync(path));
        }
        finally { Directory.Delete(directory, true); }
    }
    [Fact]
    public async Task WriteFailureIsReported()
    {
        string directory = Temp();
        try { Assert.Equal(SettingsNotice.WriteFailed, await new SettingsStore(directory).SaveAsync(new())); }
        finally { Directory.Delete(directory, true); }
    }
    [Theory]
    [InlineData(ThemeMode.System, true, true)]
    [InlineData(ThemeMode.System, false, false)]
    [InlineData(ThemeMode.Light, true, false)]
    [InlineData(ThemeMode.Dark, false, true)]
    public void ThemeSelection(ThemeMode mode, bool systemDark, bool expected) =>
        Assert.Equal(expected, AppearanceTokens.Create(new() { Theme = mode }, new(systemDark, new(1, 2, 3), false, true, true)).Dark);
    [Theory]
    [InlineData("#000000")]
    [InlineData("#FFFFFF")]
    [InlineData("#777777")]
    [InlineData("#0000FF")]
    [InlineData("#FFFF00")]
    public void AccentUsesReadableForeground(string value)
    {
        var tokens = AppearanceTokens.Create(new() { Accent = value }, new(false, new(1, 2, 3), false, true, true));
        Assert.Equal(value, tokens.Accent.ToString());
        Assert.True(Rgb.Contrast(tokens.AccentOnSurface, tokens.Surface) >= 3);
        Assert.True(Rgb.Contrast(tokens.Accent, tokens.AccentText) >= 4.5);
        Assert.True(Rgb.Contrast(tokens.Surface, tokens.Text) >= 4.5);
        Assert.True(Rgb.Contrast(tokens.Surface, tokens.Secondary) >= 4.5);
    }
    [Fact]
    public void HighContrastOverridesCustomAppearance()
    {
        var tokens = AppearanceTokens.Create(new() { Accent = "#FF0000", Opacity = .65 }, new(true, new(255, 255, 0), true, true, true, new(0, 0, 0), new(255, 255, 255), new(0, 0, 0)));
        Assert.Equal("#FFFF00", tokens.Accent.ToString());
        Assert.Equal(1, tokens.Opacity);
        Assert.False(tokens.Glass);
        Assert.False(tokens.Animate);
    }
    [Fact]
    public void DefaultsFollowSystemAccentAndTransparency()
    {
        var system = new SystemAppearance(false, new(4, 5, 6), false, false, false);
        var tokens = AppearanceTokens.Create(new(), system);
        Assert.Equal(system.Accent, tokens.Accent);
        Assert.False(tokens.Glass);
        Assert.False(tokens.Animate);
        Assert.False(AppearanceTokens.Create(new() { Glass = GlassPreference.Opaque }, system with { Transparency = true }).Glass);
    }
    [Theory]
    [InlineData(false, GlassPreference.System)]
    [InlineData(true, GlassPreference.Opaque)]
    public void OpaqueFallbackIgnoresUserSurfaceOpacity(bool transparency, GlassPreference glass)
    {
        var tokens = AppearanceTokens.Create(new() { Opacity = .65, Glass = glass }, new(false, new(0, 120, 212), false, transparency, true));
        Assert.False(tokens.Glass);
        Assert.Equal(1, tokens.Opacity);
    }
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 255)]
    [InlineData(true, 0)]
    [InlineData(true, 255)]
    public void MinimumGlassTintKeepsOpaqueWidgetLabelsReadable(bool dark, byte backdrop)
    {
        var tokens = AppearanceTokens.Create(new() { Opacity = .65 }, new(dark, new(0, 120, 212), false, true, true));
        byte Blend(byte channel) => (byte)Math.Round(channel * tokens.Opacity + backdrop * (1 - tokens.Opacity));
        var composite = new Rgb(Blend(tokens.Surface.R), Blend(tokens.Surface.G), Blend(tokens.Surface.B));
        Assert.True(Rgb.Contrast(composite, tokens.Text) >= 4.5);
        // Active segment and label are both opaque even when the surrounding surface is translucent.
        Assert.True(Rgb.Contrast(tokens.Accent, tokens.AccentText) >= 4.5);
    }
    [Theory]
    [InlineData(9000, 9000, 1800, 1000)]
    [InlineData(-9000, -9000, 0, 0)]
    [InlineData(500, 400, 500, 400)]
    public void PositionClampsToWorkingArea(int x, int y, int expectedX, int expectedY) =>
        Assert.Equal(new WidgetPosition(expectedX, expectedY), WidgetPosition.Clamp(new(x, y), [new(0, 0, 1920, 1080, 2)], 60, 40));
    [Fact]
    public void MonitorDisconnectAndNegativeCoordinatesRecover()
    {
        Assert.Equal(new WidgetPosition(-1800, 100), WidgetPosition.Clamp(new(-1800, 100), [new(-1920, 0, 1920, 1080, 1)], 120, 44));
        Assert.Equal(new WidgetPosition(0, 100), WidgetPosition.Clamp(new(-1800, 100), [new(0, 0, 1920, 1080, 1)], 120, 44));
        Assert.Equal(new WidgetPosition(40, 40), WidgetPosition.Clamp(null, [], 120, 44));
    }
    private static string Temp()
    {
        string directory = Path.Combine(Path.GetTempPath(), "layfix-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
