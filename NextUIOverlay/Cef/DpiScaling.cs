using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Xilium.CefGlue;

namespace NextUIOverlay.Cef;

public static class DpiScaling
{
    internal static float cachedDeviceScale;

    [DllImport("shcore.dll")]
    public static extern void GetScaleFactorForMonitor(IntPtr hMon, out uint pScale);

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr handle, uint dwFlags);

    public static float GetDeviceScale()
    {
        if ((double) DpiScaling.cachedDeviceScale != 0.0)
            return DpiScaling.cachedDeviceScale;
        uint pScale;
        DpiScaling.GetScaleFactorForMonitor(DpiScaling.MonitorFromWindow(IntPtr.Zero, 1U), out pScale);
        DpiScaling.cachedDeviceScale = (float) pScale / 100f;
        return DpiScaling.cachedDeviceScale;
    }

    public static CefRectangle ScaleViewRect(CefRectangle rect)
    {
        return new CefRectangle(rect.X, rect.Y, (int) Math.Ceiling((double) rect.Width * (1.0 / (double) DpiScaling.GetDeviceScale())), (int) Math.Ceiling((double) rect.Height * (1.0 / (double) DpiScaling.GetDeviceScale())));
    }

    public static CefRectangle ScaleScreenRect(CefRectangle rect)
    {
        return new CefRectangle(rect.X, rect.Y, (int) Math.Ceiling((double) rect.Width * (double) DpiScaling.GetDeviceScale()), (int) Math.Ceiling((double) rect.Height * (double) DpiScaling.GetDeviceScale()));
    }

    public static Point ScaleViewPoint(float x, float y)
    {
        return new Point((int) Math.Ceiling((double) x * (1.0 / (double) DpiScaling.GetDeviceScale())), (int) Math.Ceiling((double) y * (1.0 / (double) DpiScaling.GetDeviceScale())));
    }

    public static Point ScaleScreenPoint(float x, float y)
    {
        return new Point((int) Math.Ceiling((double) x * (double) DpiScaling.GetDeviceScale()), (int) Math.Ceiling((double) y * (double) DpiScaling.GetDeviceScale()));
    }
}