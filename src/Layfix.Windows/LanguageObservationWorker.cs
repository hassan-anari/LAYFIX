using System.Threading.Channels;
using Layfix.Core;

namespace Layfix.Windows;

/// <summary>Coalesces notification bursts; 750 ms reconciliation also catches foreign WM_INPUTLANGCHANGE.</summary>
public sealed class LanguageObservationWorker(LanguageStateService state) : IAsyncDisposable
{
    private readonly Channel<byte> signal = Channel.CreateBounded<byte>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });
    private readonly CancellationTokenSource stop = new();
    private Task? worker;
    private Task? timer;
    public void Signal() => signal.Writer.TryWrite(0);
    public void Start()
    {
        if (worker != null) throw new InvalidOperationException("Already started.");
        worker = Task.Run(async () =>
        {
            try
            {
                await foreach (byte _ in signal.Reader.ReadAllAsync(stop.Token).ConfigureAwait(false))
                    await state.RefreshAsync(stop.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        });
        timer = Task.Run(async () =>
        {
            using var periodic = new PeriodicTimer(TimeSpan.FromMilliseconds(750));
            try { while (await periodic.WaitForNextTickAsync(stop.Token).ConfigureAwait(false)) if (!state.Paused) Signal(); }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        });
        Signal();
    }
    public async ValueTask DisposeAsync()
    {
        await stop.CancelAsync().ConfigureAwait(false);
        signal.Writer.TryComplete();
        if (worker != null) await worker.ConfigureAwait(false);
        if (timer != null) await timer.ConfigureAwait(false);
        stop.Dispose();
    }
}
