using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace LayoutFixer;

static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        // LayoutFixer.exe --test "صشدف لقشذ"   checks detection without running the app.
        if (args.Length >= 2 && args[0] == "--test") return TestMode.Run(args[1]);

        using var mutex = new Mutex(true, "LayoutFixer.SingleInstance", out bool first);
        if (!first) return 0;
        ApplicationConfiguration.Initialize();
        using var app = new TrayApp();
        Application.Run(app);
        return 0;
    }
}

/// Upstream tray retained. No global hooks are installed in this audit build.
sealed class TrayApp : ApplicationContext
{
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    readonly NotifyIcon tray;
    readonly ContextMenuStrip menu = new();
    bool resourcesDisposed;
    readonly ToolStripMenuItem startupItem = new("Start with Windows");

    public TrayApp()
    {
        menu.Items.Add(new ToolStripMenuItem("LAYFIX · upstream Windows audit") { Enabled = false });
        menu.Items.Add(new ToolStripMenuItem(RuntimeSafety.Status) { Enabled = false });
        menu.Items.Add(new ToolStripMenuItem("Use --test for offline detection checks") { Enabled = false });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(startupItem);
        menu.Items.Add("Quit LAYFIX", null, (_, _) => ExitThread());
        startupItem.Click += (_, _) => SetStartWithWindows(!startupItem.Checked);
        menu.Opening += (_, _) => startupItem.Checked = StartsWithWindows();
        tray = new NotifyIcon { Icon = LoadIcon(), Text = "LAYFIX: observation blocked", ContextMenuStrip = menu, Visible = true };
    }

    static Icon LoadIcon()
    {
        using var stream = typeof(TrayApp).Assembly.GetManifestResourceStream("LayoutFixer.tray.ico");
        return stream != null ? new Icon(stream, SystemInformation.SmallIconSize) : SystemIcons.Application;
    }

    static bool StartsWithWindows()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey);
        return key?.GetValue("LayoutFixer") is string;
    }

    static void SetStartWithWindows(bool on)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (on) key.SetValue("LayoutFixer", $"\"{Environment.ProcessPath}\"");
        else key.DeleteValue("LayoutFixer", false);
    }

    void CloseResources()
    {
        if (resourcesDisposed) return;
        resourcesDisposed = true;
        tray.Visible = false;
        tray.Dispose();
        menu.Dispose();
    }

    protected override void ExitThreadCore()
    {
        CloseResources();
        base.ExitThreadCore();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) CloseResources();
        base.Dispose(disposing);
    }
}

/// Command-line check of the detection rules, using the layouts installed on this PC.
static class TestMode
{
    public static int Run(string text)
    {
        Native.AttachConsole(-1);
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var dicts = new Dictionaries(Path.Combine(AppContext.BaseDirectory, "data"));
        var layouts = Layouts.Enabled();
        Console.WriteLine("layouts: " + string.Join(", ", layouts.Select(l => $"{l.Key}={l.Value.Hkl:X}")));
        bool persian = text.Any(c => c >= 0x0600 && c <= 0x06FF);
        if (!layouts.TryGetValue(persian ? "fa" : "en", out var from) || !layouts.TryGetValue(persian ? "en" : "fa", out var to))
        {
            Console.WriteLine("need both an English and a Persian layout installed");
            return 1;
        }
        // Reverse map: character -> physical key on the source layout.
        var keyFor = new Dictionary<string, (uint vk, uint scan, bool shift)>();
        for (uint vk = 0x20; vk < 0xFF; vk++)
            foreach (var shift in new[] { false, true })
            {
                uint scan = Native.MapVirtualKeyEx(vk, 0, from.Hkl);
                if (from.Character(vk, scan, shift) is { } c) keyFor.TryAdd(c, (vk, scan, shift));
            }
        foreach (var w in text.Normalize().Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var converted = string.Concat(w.EnumerateRunes().Select(ch =>
                keyFor.TryGetValue(ch.ToString(), out var k) ? to.Character(k.vk, k.scan, k.shift) ?? ch.ToString() : ch.ToString()));
            bool fix = dicts.IsMistake(w, from.Lang, converted), shortFix = dicts.IsMistake(w, from.Lang, converted, 1);
            Console.WriteLine($"{w} -> {converted}  {(fix ? "FIX" : shortFix ? "fix-if-next-is-wrong" : "ok")}");
        }
        return 0;
    }
}
