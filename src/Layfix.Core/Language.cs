namespace Layfix.Core;

public enum InputLanguage { Unknown, English, Persian, Unsupported }
public enum ObservationStatus { Available, Unknown, Unsupported, Unavailable, Paused }
public enum SwitchOutcome { None, Pending, Succeeded, Unavailable, Unsupported, StaleContext, Blocked, TimedOut, Busy, Cancelled }

public sealed record KeyboardLayout(long Handle, string Id, string Name, InputLanguage Language)
{
    public static InputLanguage Classify(long handle) => handle == 0 ? InputLanguage.Unknown :
        (handle & 0x3ff) switch { 0x09 => InputLanguage.English, 0x29 => InputLanguage.Persian, _ => InputLanguage.Unsupported };
}

// Identity only: no title, text, caret, selection, or key data.
public sealed record TargetContext(long Foreground, long Focus, uint Thread, uint Process, long Generation);
public sealed record LanguageSnapshot(ObservationStatus Status, TargetContext? Target, KeyboardLayout? Layout)
{
    public static LanguageSnapshot Unavailable { get; } = new(ObservationStatus.Unavailable, null, null);
    public string Indicator => Status == ObservationStatus.Available ? Layout?.Language switch
    {
        InputLanguage.English => "EN", InputLanguage.Persian => "FA", _ => "?"
    } : Status switch { ObservationStatus.Paused => "Ⅱ", ObservationStatus.Unsupported => "—", ObservationStatus.Unavailable => "×", _ => "?" };
}

public interface ILanguagePlatform
{
    LanguageSnapshot Read();
    IReadOnlyList<KeyboardLayout> DiscoverLayouts();
    SwitchOutcome RequestSwitch(TargetContext expected, long sourceLayout, KeyboardLayout target);
}

public static class LayoutSelection
{
    public static KeyboardLayout? Select(IEnumerable<KeyboardLayout> layouts, InputLanguage language, string? preferredId) =>
        layouts.Where(x => x.Language == language && (preferredId == null || string.Equals(x.Id, preferredId, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault();

    public static InputLanguage Opposite(InputLanguage language) => language switch
    {
        InputLanguage.English => InputLanguage.Persian,
        InputLanguage.Persian => InputLanguage.English,
        _ => InputLanguage.Unknown
    };
}
