using Avalonia;

namespace Layfix.App;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (!OperatingSystem.IsWindows()) return;
        using var instance = new Mutex(true, @"Local\LAYFIX.Phase1", out bool first);
        if (!first)
        {
            try { using var signal = EventWaitHandle.OpenExisting(@"Local\LAYFIX.ShowSettings"); signal.Set(); }
            catch (WaitHandleCannotBeOpenedException) { /* The first instance may still be initializing. */ }
            return;
        }
        try { BuildAvaloniaApp().StartWithClassicDesktopLifetime(args); }
        finally { instance.ReleaseMutex(); }
    }
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<LayfixApplication>().UsePlatformDetect();
}
