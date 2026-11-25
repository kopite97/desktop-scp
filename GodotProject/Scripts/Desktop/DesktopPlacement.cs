using Godot;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
using System;
using System.Linq;

namespace SiteDesktop.Desktop;

public static class DesktopPlacement
{
    public static string RequestedMonitor
    {
        get
        {
            var args = OS.GetCmdlineUserArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "--monitor-id") return args[i + 1];
            return "";
        }
    }
    public static DesktopScreen Select(string id = "")
    {
        if (DisplayServer.GetName() == "headless") return new(0, new(0, 0, 1920, 1080), new(0, 0, 1920, 1032), 96);
        var screens = OverlayController.ReadScreens();
        if (id == "") id = RequestedMonitor;
        if (id == "") return screens[0];
        var monitors = WindowsMonitors.Read().Where(m => m.HardwareId.Contains(id, StringComparison.OrdinalIgnoreCase) || m.Device.Equals(id, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (monitors.Length != 1) throw new InvalidOperationException("Requested monitor must match exactly one attached display: " + id);
        var monitor = monitors[0];
        return screens.Single(s => s.Bounds.Position == new Vector2I(monitor.X, monitor.Y));
    }
}
