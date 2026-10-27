using System.Text.Json;
using System.Text.Json.Serialization;

namespace Layfix.Core;

public enum ThemeMode { System, Light, Dark }
public enum GlassPreference { System, Opaque }
public sealed record AppSettings
{
    public int SchemaVersion { get; init; } = 1;
    public bool WidgetEnabled { get; init; } = true;
    public bool WidgetVisible { get; init; } = true;
    public bool Paused { get; init; }
    public ThemeMode Theme { get; init; }
    public string? Accent { get; init; }
    public double Opacity { get; init; } = 1;
    public double Scale { get; init; } = 1;
    public GlassPreference Glass { get; init; }
    public bool DragLocked { get; init; }
    public bool AlwaysOnTop { get; init; } = true;
    public WidgetPosition? Position { get; init; }
    public string Locale { get; init; } = "en";
    public string? EnglishLayoutId { get; init; }
    public string? PersianLayoutId { get; init; }

    public AppSettings Validate() => this with
    {
        SchemaVersion = 1,
        Theme = Enum.IsDefined(Theme) ? Theme : ThemeMode.System,
        Glass = Enum.IsDefined(Glass) ? Glass : GlassPreference.System,
        Accent = Rgb.TryParse(Accent, out _) ? Accent!.ToUpperInvariant() : null,
        Opacity = double.IsFinite(Opacity) ? Math.Clamp(Opacity, .65, 1) : 1,
        Scale = double.IsFinite(Scale) ? Math.Clamp(Scale, .8, 1.8) : 1,
        Locale = Locale == "fa" ? "fa" : "en",
        Position = Position?.Validate(),
        EnglishLayoutId = ValidId(EnglishLayoutId), PersianLayoutId = ValidId(PersianLayoutId)
    };
    private static string? ValidId(string? value) => value is { Length: > 0 and <= 16 } && value.All(Uri.IsHexDigit) ? value.ToUpperInvariant() : null;
}
public enum SettingsNotice { None, Recovered, ReadFailed, WriteFailed, NewerVersion }
public sealed record SettingsLoad(AppSettings Settings, SettingsNotice Notice);

public sealed class SettingsStore(string path)
{
    private readonly SemaphoreSlim writeGate = new(1, 1);
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
    private bool readOnly;
    public async Task<SettingsLoad> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(path)) return new(new(), SettingsNotice.None);
            var info = new FileInfo(path);
            if (info.Length > 65536) return new(new(), SettingsNotice.Recovered);
            using var stream = File.OpenRead(path);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, Options, cancellationToken).ConfigureAwait(false);
            if (settings is { SchemaVersion: > 1 })
            {
                readOnly = true;
                return new(new(), SettingsNotice.NewerVersion);
            }
            return settings == null ? new(new(), SettingsNotice.Recovered) : new(settings.Validate(), SettingsNotice.None);
        }
        catch (JsonException) { return new(new(), SettingsNotice.Recovered); }
        catch (IOException) { return new(new(), SettingsNotice.ReadFailed); }
        catch (UnauthorizedAccessException) { return new(new(), SettingsNotice.ReadFailed); }
    }
    public async Task<SettingsNotice> SaveAsync(AppSettings value, CancellationToken cancellationToken = default)
    {
        if (readOnly) return SettingsNotice.NewerVersion;
        await writeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            await using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, value.Validate(), Options, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                stream.Flush(true);
            }
            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(path)) File.Replace(temp, path, null);
            else File.Move(temp, path);
            return SettingsNotice.None;
        }
        catch (IOException) { return SettingsNotice.WriteFailed; }
        catch (UnauthorizedAccessException) { return SettingsNotice.WriteFailed; }
        finally
        {
            try { if (File.Exists(temp)) File.Delete(temp); }
            catch (IOException) { /* A failed cleanup cannot invalidate the last committed settings. */ }
            catch (UnauthorizedAccessException) { }
            writeGate.Release();
        }
    }
}
