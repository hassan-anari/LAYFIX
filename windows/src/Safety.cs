namespace LayoutFixer;

// A provider must establish every fact independently of the keyboard hook.
sealed record ContextFacts(bool Password, bool Sensitive, bool Editable, bool Supported,
    bool DefaultDesktop, bool SameIntegrity, bool Remote, bool Certain);

static class SafetyPolicy
{
    public static bool MayObserve(ContextFacts facts) =>
        facts.Certain && facts.Supported && facts.Editable && facts.DefaultDesktop &&
        facts.SameIntegrity && !facts.Password && !facts.Sensitive && !facts.Remote;
}

sealed record EditContext(nint Foreground, nint Focus, uint Thread, nint Hkl, long Generation)
{
    public bool IsKnown => Foreground != 0 && Focus != 0 && Thread != 0 && Hkl != 0 && Generation > 0;
}

sealed record EditTicket(EditContext Context, int Start, int Length, long Revision);
enum ReplacementResult { Rejected, Failed, Partial, Success }

interface ICorrectionBoundary
{
    EditContext? Capture();
    bool MayObserve(EditContext context);
    bool CanReplace { get; }
    bool TryValidateSpan(EditContext context, string expected, out EditTicket? ticket);
    ReplacementResult Replace(EditTicket ticket, string replacement);
    bool SwitchLayout(EditContext context, Layout target);
}

interface ISuggestionPopup
{
    bool Visible { get; }
    void ShowSuggestion(string suggestion, string target);
    void HideIt();
}

interface ISuggestionTimer : IDisposable
{
    void Restart();
    void Stop();
}

sealed record ObservedKey(uint Vk, uint Scan, bool Shift = false, bool CapsLock = false,
    bool Shortcut = false, bool Injected = false);

// No cross-application editor adapter has passed privacy and atomic-span acceptance.
// This immutable shipping gate is intentionally not a command-line/settings override.
// Do not install global hooks until the gate is replaced by a reviewed adapter registry.
static class RuntimeSafety
{
    public static bool GlobalObservationApproved => false;
    public const string Status = "Observation and correction blocked: editor safety acceptance pending";
}

// Registration is transactional, including a failed second/third registration.
sealed class HookLifetime : IDisposable
{
    readonly List<Action> cleanup = [];
    bool disposed;

    public void Register(Func<nint> acquire, Action<nint> release)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var handle = acquire();
        if (handle == 0)
        {
            Dispose();
            throw new InvalidOperationException("Hook registration failed");
        }
        cleanup.Add(() => release(handle));
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        try
        {
            for (int i = cleanup.Count - 1; i >= 0; i--) cleanup[i]();
        }
        finally { cleanup.Clear(); }
    }
}
