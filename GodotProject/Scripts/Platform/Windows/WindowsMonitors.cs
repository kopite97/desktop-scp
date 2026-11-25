using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SiteDesktop.Platform.Windows;

public sealed record MonitorDetails(string Device, string HardwareId, string Name,
    int X, int Y, int Width, int Height, int WorkX, int WorkY, int WorkWidth, int WorkHeight);

public static class WindowsMonitors
{
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MonitorInfo
    {
        public int Size; public Rect Bounds, Work; public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string Device;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DisplayDevice
    {
        public int Size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string Device;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Name;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Key;
    }
    private delegate bool MonitorCallback(nint monitor, nint dc, ref Rect rect, nint data);
    [DllImport("user32.dll")] private static extern bool EnumDisplayMonitors(nint dc, nint clip, MonitorCallback callback, nint data);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern bool GetMonitorInfoW(nint monitor, ref MonitorInfo info);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern bool EnumDisplayDevicesW(string device, uint index, ref DisplayDevice info, uint flags);

    public static MonitorDetails[] Read()
    {
        var items = new List<MonitorDetails>();
        EnumDisplayMonitors(0, 0, (nint monitor, nint dc, ref Rect rect, nint data) =>
        {
            var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
            if (!GetMonitorInfoW(monitor, ref info)) return true;
            var device = new DisplayDevice { Size = Marshal.SizeOf<DisplayDevice>() };
            EnumDisplayDevicesW(info.Device, 0, ref device, 1);
            var b = info.Bounds; var w = info.Work;
            items.Add(new(info.Device, device.Id ?? "", device.Name ?? "", b.Left, b.Top, b.Right - b.Left, b.Bottom - b.Top,
                w.Left, w.Top, w.Right - w.Left, w.Bottom - w.Top));
            return true;
        }, 0);
        return items.ToArray();
    }
}
