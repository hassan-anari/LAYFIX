using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Layfix.Windows;

/// <summary>A dedicated STA pumps out-of-context WinEvents and a hidden broadcast window. No keyboard hooks.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsNotifications : IDisposable
{
    private readonly Thread thread;
    private readonly TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly NativeMethods.WinEventProc eventProc;
    private readonly NativeMethods.WindowProc windowProc;
    private NativeRegistrationSet? hooks;
    private nint window;
    private long generation;
    private volatile bool paused;
    private volatile bool locked;
    private volatile bool sleeping;
    private bool disposed;
    private int hookCount;
    public int ActiveHookCount => Volatile.Read(ref hookCount);
    public bool SessionNotificationsAvailable { get; private set; }
    public event Action? Changed;
    public event Action? AppearanceChanged;
    public long Generation => Interlocked.Read(ref generation);
    public bool Suspended => locked || sleeping;
    public WindowsNotifications()
    {
        eventProc = OnEvent;
        windowProc = OnMessage;
        thread = new Thread(Run) { IsBackground = true, Name = "LAYFIX native notifications" };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }
    public Task Ready => started.Task;
    public void SetPaused(bool value)
    {
        if (paused == value) return;
        paused = value;
        if (window != 0) NativeMethods.PostMessageW(window, 0x8001, 0, 0);
    }
    private void Invalidate()
    {
        Interlocked.Increment(ref generation);
        Changed?.Invoke();
    }
    private void OnEvent(nint hook, uint type, nint hwnd, int objectId, int childId, uint eventThread, uint time)
    {
        if (!paused) Invalidate();
    }
    private void UpdateHooks()
    {
        hooks?.Dispose();
        hooks = null;
        Volatile.Write(ref hookCount, 0);
        if (paused) return;
        var registrations = new NativeRegistrationSet();
        try
        {
            foreach (uint id in new uint[] { 3, 0x8005, 0x20 })
            {
                nint handle = NativeMethods.SetWinEventHook(id, id, 0, eventProc, 0, 0, 2);
                if (handle == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
                registrations.Add(() => NativeMethods.UnhookWinEvent(handle));
            }
            hooks = registrations;
            Volatile.Write(ref hookCount, 3);
        }
        catch (Win32Exception)
        {
            registrations.Dispose();
            // Reconciliation remains available if the OS refuses an event registration.
        }
        Invalidate();
    }
    private nint OnMessage(nint hwnd, uint message, nint wparam, nint lparam)
    {
        switch (message)
        {
            case 0x8001: UpdateHooks(); break;
            case 0x0010: NativeMethods.DestroyWindow(hwnd); return 0;
            case 0x0002: NativeMethods.PostQuitMessage(0); return 0;
            case 0x02B1:
                if ((int)wparam is 7 or 4) locked = true;
                if ((int)wparam is 8 or 3) locked = false;
                Invalidate(); break;
            case 0x0218:
                if ((int)wparam == 4) sleeping = true;
                if ((int)wparam is 7 or 18) sleeping = false;
                Invalidate(); break;
            case 0x001A:
            case 0x031A:
            case 0x0320:
            case 0x007E:
            case 0x0051:
                AppearanceChanged?.Invoke();
                Invalidate(); break;
        }
        return NativeMethods.DefWindowProcW(hwnd, message, wparam, lparam);
    }
    private void Run()
    {
        string name = "LAYFIX.Notifications." + Environment.ProcessId;
        nint instance = NativeMethods.GetModuleHandleW(null);
        bool registered = false;
        bool session = false;
        try
        {
            var cls = new NativeMethods.WindowClass { Size = (uint)Marshal.SizeOf<NativeMethods.WindowClass>(), ClassName = name, Procedure = windowProc, Instance = instance };
            if (NativeMethods.RegisterClassExW(ref cls) == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
            registered = true;
            window = NativeMethods.CreateWindowExW(0x08000080, name, "", 0, 0, 0, 0, 0, 0, 0, instance, 0);
            if (window == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
            session = NativeMethods.WTSRegisterSessionNotification(window, 0);
            SessionNotificationsAvailable = session;
            UpdateHooks();
            started.SetResult();
            while (NativeMethods.GetMessageW(out var message, 0, 0, 0) > 0)
            {
                NativeMethods.TranslateMessage(ref message);
                NativeMethods.DispatchMessageW(ref message);
            }
        }
        catch (Win32Exception ex) { started.TrySetException(ex); }
        finally
        {
            hooks?.Dispose();
            hooks = null;
            Volatile.Write(ref hookCount, 0);
            if (session) NativeMethods.WTSUnRegisterSessionNotification(window);
            if (window != 0) NativeMethods.DestroyWindow(window);
            window = 0;
            if (registered) NativeMethods.UnregisterClassW(name, instance);
        }
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        if (window != 0) NativeMethods.PostMessageW(window, 0x0010, 0, 0);
        thread.Join();
        Changed = null;
        AppearanceChanged = null;
        GC.KeepAlive(eventProc);
        GC.KeepAlive(windowProc);
    }
}
