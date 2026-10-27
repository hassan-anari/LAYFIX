namespace Layfix.Core;

/// <summary>Serialized, observable state. Platform reads are performed by the worker, never UI callbacks.</summary>
public sealed class LanguageStateService(ILanguagePlatform platform, Func<TimeSpan, CancellationToken, Task>? delay = null) : IAsyncDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly CancellationTokenSource lifetime = new();
    private readonly Func<TimeSpan, CancellationToken, Task> wait = delay ?? Task.Delay;
    private volatile LanguageSnapshot current = LanguageSnapshot.Unavailable;
    private volatile bool paused;
    private volatile bool disposed;
    private int switching;
    private long pauseGeneration;
    public event Action<LanguageSnapshot>? Changed;
    public event Action<SwitchOutcome>? SwitchChanged;
    public LanguageSnapshot Current => current;
    public SwitchOutcome LastSwitch { get; private set; }
    public bool Paused => paused;

    private void Publish(LanguageSnapshot value)
    {
        if (current == value) return;
        current = value;
        Changed?.Invoke(value);
    }
    private SwitchOutcome Outcome(SwitchOutcome value)
    {
        LastSwitch = value;
        SwitchChanged?.Invoke(value);
        return value;
    }
    public void SetPaused(bool value)
    {
        if (paused == value) return;
        Interlocked.Increment(ref pauseGeneration);
        paused = value;
        // An in-flight operation checks this before posting and on every readback.
        if (value) Publish(new(ObservationStatus.Paused, null, null));
    }
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (disposed || paused) return;
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifetime.Token);
        await gate.WaitAsync(linked.Token).ConfigureAwait(false);
        try { if (!paused && !disposed) Publish(platform.Read()); }
        finally { gate.Release(); }
    }
    public async Task<SwitchOutcome> SwitchAsync(LanguageSnapshot captured, string? englishId, string? persianId, CancellationToken cancellationToken = default)
    {
        if (disposed || paused) return SwitchOutcome.Cancelled;
        if (Interlocked.CompareExchange(ref switching, 1, 0) != 0) return SwitchOutcome.Busy;
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifetime.Token);
        long pauseVersion = Interlocked.Read(ref pauseGeneration);
        bool acquired = false;
        try
        {
            await gate.WaitAsync(linked.Token).ConfigureAwait(false);
            acquired = true;
            if (paused || disposed || pauseVersion != Interlocked.Read(ref pauseGeneration)) return Outcome(SwitchOutcome.Cancelled);
            if (captured.Status != ObservationStatus.Available || captured.Target == null || captured.Layout == null)
                return Outcome(captured.Status == ObservationStatus.Unsupported ? SwitchOutcome.Unsupported : SwitchOutcome.Unavailable);
            var before = platform.Read();
            Publish(before);
            if (before != captured) return Outcome(SwitchOutcome.StaleContext);
            var language = LayoutSelection.Opposite(captured.Layout.Language);
            if (language == InputLanguage.Unknown) return Outcome(SwitchOutcome.Unsupported);
            var target = LayoutSelection.Select(platform.DiscoverLayouts(), language, language == InputLanguage.English ? englishId : persianId);
            if (target == null) return Outcome(SwitchOutcome.Unavailable);
            Outcome(SwitchOutcome.Pending);
            if (paused || pauseVersion != Interlocked.Read(ref pauseGeneration) || linked.IsCancellationRequested) return Outcome(SwitchOutcome.Cancelled);
            var request = platform.RequestSwitch(captured.Target, captured.Layout.Handle, target);
            if (request != SwitchOutcome.Pending) return Outcome(request);
            // One request; subsequent iterations only verify. The HWND is never retargeted.
            for (int i = 0; i < 8; i++)
            {
                await wait(TimeSpan.FromMilliseconds(100), linked.Token).ConfigureAwait(false);
                if (paused || disposed || pauseVersion != Interlocked.Read(ref pauseGeneration)) return Outcome(SwitchOutcome.Cancelled);
                var actual = platform.Read();
                Publish(actual);
                if (actual.Target != captured.Target) return Outcome(SwitchOutcome.StaleContext);
                if (actual.Status != ObservationStatus.Available) return Outcome(SwitchOutcome.Unavailable);
                if (actual.Layout?.Handle == target.Handle) return Outcome(SwitchOutcome.Succeeded);
                if (actual.Layout?.Handle != captured.Layout.Handle) return Outcome(SwitchOutcome.Cancelled);
            }
            return Outcome(SwitchOutcome.TimedOut);
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested) { return Outcome(SwitchOutcome.Cancelled); }
        finally
        {
            if (acquired) gate.Release();
            Interlocked.Exchange(ref switching, 0);
        }
    }
    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        disposed = true;
        await lifetime.CancelAsync().ConfigureAwait(false);
        await gate.WaitAsync().ConfigureAwait(false);
        gate.Release();
        Changed = null;
        SwitchChanged = null;
        // The owner stops and joins the worker before disposing the synchronization objects.
        lifetime.Dispose();
    }
}
