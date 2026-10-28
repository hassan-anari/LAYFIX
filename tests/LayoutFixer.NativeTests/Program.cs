using System.Runtime.InteropServices;

namespace LayoutFixer;

static class NativeTests
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern nint LoadKeyboardLayout(string klid, uint flags);
    [DllImport("user32.dll")]
    static extern bool UnloadKeyboardLayout(nint hkl);

    static int Main(string[] args)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        var tests = new TestRunner();
        tests.Add("native INPUT ABI is correct for x64", () => Check.Equal(40, Marshal.SizeOf<Native.INPUT>()));
        tests.Add("native hook ABI is correct for x64", () => Check.Equal(24, Marshal.SizeOf<Native.KBDLLHOOKSTRUCT>()));
        tests.Add("native GUI thread ABI is correct for x64", () => Check.Equal(72, Marshal.SizeOf<Native.GUITHREADINFO>()));
        tests.Add("installed layout enumeration has no zero handles", () => Check.True(new WindowsLayoutPlatform().Installed().All(h => h != 0)));
        tests.Add("real hook registers and tears down without collecting events", () =>
        {
            // Callback deliberately never dereferences lParam; no input or text is read.
            Native.HookProc callback = (code, wp, lp) => Native.CallNextHookEx(0, code, wp, lp);
            using var hooks = new HookLifetime();
            hooks.Register(() => Native.SetWindowsHookEx(Native.WH_KEYBOARD_LL, callback, Native.GetModuleHandle(null), 0),
                h => Check.True(Native.UnhookWindowsHookEx(h)));
            hooks.Dispose();
            GC.KeepAlive(callback);
        });
        tests.Add("native US physical mapping Shift and Caps Lock", () => WithLayout("00000409", h =>
        {
            var l = new Layout(h, "en");
            Check.Equal("a", l.Character(0x41, 0x1E, false));
            Check.Equal("A", l.Character(0x41, 0x1E, true));
            Check.Equal("A", l.Character(0x41, 0x1E, false, true));
            Check.Equal("a", l.Character(0x41, 0x1E, true, true));
            Check.Equal("!", l.Character(0x31, 0x02, true));
        }));
        foreach (var klid in new[] { "00000429", "00050429" })
            tests.Add("native Persian physical mapping " + klid, () => WithLayout(klid, h =>
            {
                var l = new Layout(h, "fa");
                Check.Equal("fa", Layouts.LangOf(h));
                Check.Equal("سلام", string.Concat(new uint[] { 0x1F, 0x22, 0x23, 0x26 }.Select(scan => l.Character(0, scan, false))));
            }));
        return tests.Run(args.FirstOrDefault() ?? "TestResults/native.xml");
    }

    static void WithLayout(string klid, Action<nint> check)
    {
        var existing = new WindowsLayoutPlatform().Installed();
        var hkl = LoadKeyboardLayout(klid, 0); // No KLF_ACTIVATE; never generates input.
        Check.True(hkl != 0);
        try { check(hkl); }
        finally { if (!existing.Contains(hkl)) Check.True(UnloadKeyboardLayout(hkl)); }
    }
}
