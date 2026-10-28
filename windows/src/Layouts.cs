namespace LayoutFixer;

interface IKeyboardMapping
{
    uint VirtualKey(uint scan, nint hkl);
    string? Translate(uint vk, uint scan, bool shift, bool capsLock, nint hkl);
}

sealed class WindowsKeyboardMapping : IKeyboardMapping
{
    public uint VirtualKey(uint scan, nint hkl) => Native.MapVirtualKeyEx(scan, 3, hkl);

    public string? Translate(uint vk, uint scan, bool shift, bool capsLock, nint hkl)
    {
        var state = new byte[256];
        if (shift) state[0x10] = 0x80;
        if (capsLock) state[0x14] = 1;
        var buffer = new char[16];
        // Do not mutate the caller's dead-key state (Windows 10 1607+).
        int n = Native.ToUnicodeEx(vk, scan, state, buffer, buffer.Length, 4, hkl);
        return n > 0 && n < buffer.Length ? new string(buffer, 0, n) : null;
    }
}

/// The physical scan code is resolved in the destination layout, retaining Shift/Caps.
sealed record Layout(nint Hkl, string Lang, IKeyboardMapping? Mapping = null)
{
    public string DisplayName => Lang == "fa" ? "فارسی" : "English";
    public string? Character(uint vk, uint scan, bool shift, bool capsLock = false)
    {
        var api = Mapping ?? new WindowsKeyboardMapping();
        uint physicalVk = scan == 0 ? vk : api.VirtualKey(scan, Hkl);
        if (physicalVk == 0 || Hkl == 0) return null;
        var result = api.Translate(physicalVk, scan, shift, capsLock, Hkl);
        if (string.IsNullOrEmpty(result) || result.Any(char.IsControl)) return null;
        // Refuse malformed UTF-16; valid multi-unit/combining sequences remain intact.
        for (int i = 0; i < result.Length; i++)
            if (char.IsSurrogate(result[i]) &&
                (!char.IsHighSurrogate(result[i]) || ++i >= result.Length || !char.IsLowSurrogate(result[i]))) return null;
        return result;
    }
}

interface ILayoutPlatform
{
    IReadOnlyList<nint> Installed();
    bool TryFocusedThread(out nint foreground, out nint focus, out uint thread);
    nint KeyboardLayout(uint thread);
}

sealed class WindowsLayoutPlatform : ILayoutPlatform
{
    public IReadOnlyList<nint> Installed()
    {
        int n = Native.GetKeyboardLayoutList(0, null);
        if (n <= 0 || n > 256) return [];
        var list = new nint[n];
        int actual = Native.GetKeyboardLayoutList(n, list);
        return actual > 0 && actual <= n ? list.Take(actual).ToArray() : [];
    }

    public bool TryFocusedThread(out nint foreground, out nint focus, out uint thread)
    {
        foreground = Native.GetForegroundWindow();
        focus = 0;
        thread = 0;
        if (foreground == 0) return false;
        var foregroundThread = Native.GetWindowThreadProcessId(foreground, out _);
        if (foregroundThread == 0) return false;
        var info = new Native.GUITHREADINFO { cbSize = System.Runtime.InteropServices.Marshal.SizeOf<Native.GUITHREADINFO>() };
        if (!Native.GetGUIThreadInfo(foregroundThread, ref info) || info.hwndFocus == 0) return false;
        focus = info.hwndFocus;
        thread = Native.GetWindowThreadProcessId(focus, out _);
        return thread != 0 && Native.GetForegroundWindow() == foreground;
    }

    public nint KeyboardLayout(uint thread) => thread == 0 ? 0 : Native.GetKeyboardLayout(thread);
}

static class Layouts
{
    public static string? LangOf(nint hkl) => ((long)hkl & 0x3FF) switch { 0x29 => "fa", 0x09 => "en", _ => null };

    public static IReadOnlyList<Layout> All(ILayoutPlatform? platform = null) =>
        (platform ?? new WindowsLayoutPlatform()).Installed().Distinct()
        .Where(h => h != 0 && LangOf(h) is not null).Select(h => new Layout(h, LangOf(h)!)).ToArray();

    // Multiple opposite variants require explicit future profile selection. Never pick the first.
    public static Layout? Other(Layout current, ILayoutPlatform? platform = null)
    {
        var candidates = All(platform).Where(l => l.Lang != current.Lang).ToArray();
        return candidates.Length == 1 ? candidates[0] : null;
    }

    public static Layout? Current(ILayoutPlatform? platform = null)
    {
        var api = platform ?? new WindowsLayoutPlatform();
        if (!api.TryFocusedThread(out var foreground, out var focus, out var thread) || foreground == 0 || focus == 0 || thread == 0) return null;
        var hkl = api.KeyboardLayout(thread);
        return hkl != 0 && LangOf(hkl) is { } lang ? new Layout(hkl, lang) : null;
    }

    public static Dictionary<string, Layout> Enabled() => All().GroupBy(l => l.Lang)
        .Where(g => g.Count() == 1).ToDictionary(g => g.Key, g => g.Single());
}
