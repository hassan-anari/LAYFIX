using Layfix.Core;

namespace Layfix.Windows;

public readonly record struct NativeTarget(long Foreground, long Focus, uint ForegroundThread, uint FocusThread, uint Process);
public interface IWindowsInputApi
{
    long Generation { get; }
    bool Available { get; }
    NativeTarget? ResolveTarget();
    bool IsTargetAllowed(NativeTarget target);
    long GetLayout(uint thread);
    bool IsInputMethod(long layout);
    IReadOnlyList<KeyboardLayout> GetLayouts();
    SwitchOutcome SendLayoutRequest(NativeTarget target, long layout);
}

/// <summary>Injectable native boundary; no own-thread layout is used as a foreground substitute.</summary>
public sealed class WindowsLanguagePlatform(IWindowsInputApi api) : ILanguagePlatform
{
    public LanguageSnapshot Read()
    {
        long generation = api.Generation;
        if (!api.Available) return LanguageSnapshot.Unavailable;
        var target = api.ResolveTarget();
        if (target is not { Foreground: not 0, Focus: not 0, FocusThread: not 0, Process: not 0 } t) return LanguageSnapshot.Unavailable;
        var context = new TargetContext(t.Foreground, t.Focus, t.FocusThread, t.Process, generation);
        if (!api.IsTargetAllowed(t)) return new(ObservationStatus.Unsupported, context, null);
        long handle = api.GetLayout(t.FocusThread);
        if (generation != api.Generation || api.ResolveTarget() != target) return new(ObservationStatus.Unknown, null, null);
        if (handle == 0) return new(ObservationStatus.Unknown, context, null);
        if (t.ForegroundThread != t.FocusThread && api.GetLayout(t.ForegroundThread) != handle)
            return new(ObservationStatus.Unknown, context, null);
        if (generation != api.Generation || api.ResolveTarget() != target) return new(ObservationStatus.Unknown, null, null);
        var language = KeyboardLayout.Classify(handle);
        if (api.IsInputMethod(handle) || language == InputLanguage.Unsupported)
            return new(ObservationStatus.Unsupported, context, null);
        var layout = new KeyboardLayout(handle, unchecked((uint)handle).ToString("X8"), language.ToString(), language);
        return new(ObservationStatus.Available, context, layout);
    }
    public IReadOnlyList<KeyboardLayout> DiscoverLayouts() => api.GetLayouts();
    public SwitchOutcome RequestSwitch(TargetContext expected, long sourceLayout, KeyboardLayout target)
    {
        if (target.Language is not (InputLanguage.English or InputLanguage.Persian) || api.IsInputMethod(target.Handle)) return SwitchOutcome.Unsupported;
        if (!api.GetLayouts().Any(l => l.Handle == target.Handle && l.Language == target.Language)) return SwitchOutcome.Unavailable;
        var actual = Read();
        if (actual.Target != expected || actual.Layout?.Handle != sourceLayout) return SwitchOutcome.StaleContext;
        if (actual.Status != ObservationStatus.Available) return SwitchOutcome.Unsupported;
        var native = api.ResolveTarget();
        if (native == null || api.Generation != expected.Generation || native.Value.Foreground != expected.Foreground ||
            native.Value.Focus != expected.Focus || native.Value.FocusThread != expected.Thread || native.Value.Process != expected.Process)
            return SwitchOutcome.StaleContext;
        return api.SendLayoutRequest(native.Value, target.Handle);
    }
}
