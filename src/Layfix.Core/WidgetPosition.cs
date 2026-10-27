namespace Layfix.Core;

public sealed record MonitorBounds(int X, int Y, int Width, int Height, double Scale);
public sealed record WidgetPosition(int X, int Y)
{
    public WidgetPosition? Validate() => Math.Abs((long)X) <= 100000 && Math.Abs((long)Y) <= 100000 ? this : null;
    public static WidgetPosition Clamp(WidgetPosition? position, IReadOnlyList<MonitorBounds> monitors, double logicalWidth, double logicalHeight)
    {
        if (monitors.Count == 0) return position ?? new(40, 40);
        var point = position ?? new(monitors[0].X + 40, monitors[0].Y + 40);
        var screen = monitors.MinBy(m => Distance(point, m))!;
        int width = Math.Min(screen.Width, (int)Math.Ceiling(logicalWidth * screen.Scale));
        int height = Math.Min(screen.Height, (int)Math.Ceiling(logicalHeight * screen.Scale));
        return new(Math.Clamp(point.X, screen.X, screen.X + Math.Max(0, screen.Width - width)),
            Math.Clamp(point.Y, screen.Y, screen.Y + Math.Max(0, screen.Height - height)));
    }
    private static double Distance(WidgetPosition p, MonitorBounds m)
    {
        double dx = Math.Max(Math.Max(m.X - p.X, 0), (double)p.X - (m.X + m.Width));
        double dy = Math.Max(Math.Max(m.Y - p.Y, 0), (double)p.Y - (m.Y + m.Height));
        return dx * dx + dy * dy;
    }
}
