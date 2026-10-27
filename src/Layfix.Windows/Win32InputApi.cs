using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Layfix.Core;
using Microsoft.Win32;

namespace Layfix.Windows;

[SupportedOSPlatform("windows")]
public sealed class Win32InputApi(WindowsNotifications notifications) : IWindowsInputApi
{
    private readonly uint ownProcess = (uint)Environment.ProcessId;
    private readonly uint? ownIntegrity = Integrity((uint)Environment.ProcessId);
    public long Generation => notifications.Generation;
    public bool Available => notifications.ActiveHookCount == 3 && notifications.SessionNotificationsAvailable && !notifications.Suspended && NativeMethods.GetSystemMetrics(0x1000) == 0 && IsDefaultDesktop();
    public NativeTarget? ResolveTarget()
    {
        nint foreground = NativeMethods.GetForegroundWindow();
        if (foreground == 0) return null;
        uint thread = NativeMethods.GetWindowThreadProcessId(foreground, out uint process);
        if (thread == 0 || process == ownProcess) return null;
        var info = new NativeMethods.GuiThreadInfo { Size = (uint)Marshal.SizeOf<NativeMethods.GuiThreadInfo>() };
        if (!NativeMethods.GetGUIThreadInfo(thread, ref info) || info.Focus == 0) return null;
        uint focusThread = NativeMethods.GetWindowThreadProcessId(info.Focus, out uint focusProcess);
        if (focusThread == 0 || focusProcess != process || foreground != NativeMethods.GetForegroundWindow()) return null;
        return new((long)foreground, (long)info.Focus, thread, focusThread, process);
    }
    public bool IsTargetAllowed(NativeTarget target)
    {
        uint? integrity = Integrity(target.Process);
        return ownIntegrity != null && integrity != null && integrity <= ownIntegrity;
    }
    public long GetLayout(uint thread) => thread == 0 ? 0 : (long)NativeMethods.GetKeyboardLayout(thread);
    public bool IsInputMethod(long layout) => NativeMethods.ImmIsIME((nint)layout) || (unchecked((uint)layout) & 0xF0000000) == 0xE0000000;
    public IReadOnlyList<KeyboardLayout> GetLayouts()
    {
        int count = NativeMethods.GetKeyboardLayoutList(0, null);
        if (count is <= 0 or > 512) return [];
        var handles = new nint[count];
        count = NativeMethods.GetKeyboardLayoutList(handles.Length, handles);
        var result = new List<KeyboardLayout>();
        foreach (nint handle in handles.Take(Math.Min(count, handles.Length)).Distinct())
        {
            var language = KeyboardLayout.Classify((long)handle);
            string id = unchecked((uint)handle).ToString("X8", CultureInfo.InvariantCulture);
            string name;
            try { name = CultureInfo.GetCultureInfo((int)((long)handle & 0xffff)).EnglishName; }
            catch (CultureNotFoundException) { name = "Unsupported layout"; }
            string? klid = FindLayoutId(unchecked((uint)handle));
            if (klid != null)
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Keyboard Layouts\" + klid);
                name = key?.GetValue("Layout Text") as string ?? name;
                id = klid;
            }
            result.Add(new((long)handle, id, name + " (" + id + ")", IsInputMethod((long)handle) ? InputLanguage.Unsupported : language));
        }
        return result;
    }
    private static string? FindLayoutId(uint hkl)
    {
        uint high = hkl >> 16;
        if ((high & 0xf000) != 0xf000) return high.ToString("X8", CultureInfo.InvariantCulture);
        using var layouts = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Keyboard Layouts");
        if (layouts == null) return null;
        foreach (string id in layouts.GetSubKeyNames())
        {
            using var key = layouts.OpenSubKey(id);
            if (uint.TryParse(key?.GetValue("Layout Id") as string, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint layoutId) && layoutId == (high & 0xfff)) return id;
        }
        return null;
    }
    public SwitchOutcome SendLayoutRequest(NativeTarget target, long layout)
    {
        // Final native identity check. No AttachThreadInput, input synthesis, global activation or privilege bypass.
        if (!Available || ResolveTarget() != target) return SwitchOutcome.StaleContext;
        Marshal.SetLastPInvokeError(0);
        nint sent = NativeMethods.SendMessageTimeoutW((nint)target.Focus, 0x0050, 0, (nint)layout, 0x0002 | 0x0020, 100, out _);
        if (sent == 0) return Marshal.GetLastPInvokeError() == 5 ? SwitchOutcome.Blocked : SwitchOutcome.TimedOut;
        return SwitchOutcome.Pending;
    }
    private static bool IsDefaultDesktop()
    {
        nint desktop = NativeMethods.OpenInputDesktop(0, false, 1);
        if (desktop == 0) return false;
        try
        {
            var name = new StringBuilder(128);
            return NativeMethods.GetUserObjectInformationW(desktop, 2, name, 256, out _) && name.ToString() == "Default";
        }
        finally { NativeMethods.CloseDesktop(desktop); }
    }
    private static uint? Integrity(uint processId)
    {
        nint process = NativeMethods.OpenProcess(0x1000, false, processId);
        if (process == 0) return null;
        nint token = 0;
        nint buffer = 0;
        try
        {
            if (!NativeMethods.OpenProcessToken(process, 8, out token)) return null;
            NativeMethods.GetTokenInformation(token, 25, 0, 0, out uint needed);
            if (needed is 0 or > 65536) return null;
            buffer = Marshal.AllocHGlobal((int)needed);
            if (!NativeMethods.GetTokenInformation(token, 25, buffer, needed, out _)) return null;
            nint sid = Marshal.ReadIntPtr(buffer);
            byte count = Marshal.ReadByte(NativeMethods.GetSidSubAuthorityCount(sid));
            return count == 0 ? null : unchecked((uint)Marshal.ReadInt32(NativeMethods.GetSidSubAuthority(sid, (uint)count - 1)));
        }
        finally
        {
            if (buffer != 0) Marshal.FreeHGlobal(buffer);
            if (token != 0) NativeMethods.CloseHandle(token);
            NativeMethods.CloseHandle(process);
        }
    }
}
