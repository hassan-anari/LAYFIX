using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Layfix.Core;
using Microsoft.Win32;

namespace Layfix.Windows;

[SupportedOSPlatform("windows")]
public static class WindowsAppearance
{
    public static SystemAppearance Read(bool dark, Rgb platformAccent)
    {
        var hc = new NativeMethods.HighContrast { Size = (uint)Marshal.SizeOf<NativeMethods.HighContrast>() };
        bool highContrast = NativeMethods.SystemParametersInfoW(0x0042, hc.Size, ref hc, 0) && (hc.Flags & 1) != 0;
        bool composition = NativeMethods.DwmIsCompositionEnabled(out bool enabled) == 0 && enabled;
        bool transparency = false;
        using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            transparency = key?.GetValue("EnableTransparency") is int value && value != 0;
        NativeMethods.SystemParametersInfoBool(0x1042, 0, out int animations, 0);
        var accent = platformAccent;
        if (NativeMethods.DwmGetColorizationColor(out uint color, out _) == 0)
            accent = new((byte)(color >> 16), (byte)(color >> 8), (byte)color);
        if (highContrast) accent = SysColor(13);
        return new(dark, accent, highContrast, composition && transparency && NativeMethods.GetSystemMetrics(0x1000) == 0,
            animations != 0, SysColor(5), SysColor(8), SysColor(14));
    }
    private static Rgb SysColor(int index)
    {
        uint color = NativeMethods.GetSysColor(index);
        return new((byte)color, (byte)(color >> 8), (byte)(color >> 16));
    }
    public static void ClipPill(nint hwnd, int width, int height)
    {
        nint region = NativeMethods.CreateRoundRectRgn(0, 0, width + 1, height + 1, height, height);
        if (region != 0 && NativeMethods.SetWindowRgn(hwnd, region, true) == 0) NativeMethods.DeleteObject(region);
        // A successful SetWindowRgn transfers ownership to Windows.
    }
    public static bool ConfigureNonActivating(nint hwnd)
    {
        nint style = NativeMethods.GetWindowLongPtr(hwnd, -20);
        Marshal.SetLastPInvokeError(0);
        nint previous = NativeMethods.SetWindowLongPtr(hwnd, -20, style | 0x08000000 | 0x80);
        bool success = previous != 0 || Marshal.GetLastPInvokeError() == 0;
        return success && NativeMethods.SetWindowPos(hwnd, 0, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0004 | 0x0010 | 0x0020);
    }
}
