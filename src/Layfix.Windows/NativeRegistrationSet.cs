namespace Layfix.Windows;

/// <summary>Owns registrations even when a later registration fails. Dispose on the registering thread.</summary>
public sealed class NativeRegistrationSet : IDisposable
{
    private readonly Stack<Action> cleanup = new();
    private bool disposed;
    public void Add(Action release)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        cleanup.Push(release);
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        while (cleanup.TryPop(out var release)) release();
    }
}
