using Godot;
using SiteDesktop.Platforms;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SiteDesktop.Platform.Windows;

public sealed class WindowsWindowObserver
{
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    private delegate bool WindowCallback(nint handle, nint data);
    [DllImport("user32.dll")] private static extern bool EnumWindows(WindowCallback callback, nint data);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(nint handle);
    [DllImport("user32.dll")] private static extern bool IsIconic(nint handle);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(nint handle, out Rect rect);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint handle, out uint pid);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassNameW(nint handle, StringBuilder name, int length);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")] private static extern nint GetWindowLongPtr(nint handle, int index);
    [DllImport("dwmapi.dll", EntryPoint = "DwmGetWindowAttribute")] private static extern int GetBounds(nint handle, int attribute, out Rect value, int size);
    [DllImport("dwmapi.dll", EntryPoint = "DwmGetWindowAttribute")] private static extern int GetInteger(nint handle, int attribute, out int value, int size);
    private readonly Dictionary<(long Handle, uint Pid), long> _identities = new();
    private long _generation;
    public HashSet<nint> IncludedOwnedWindows { get; } = new();

    public string? IdFor(nint handle)
    {
        GetWindowThreadProcessId(handle, out uint pid);
        return _identities.TryGetValue(((long)handle, pid), out long generation) ? $"window:{pid}:{(long)handle}:{generation}" : null;
    }

    public WindowObservation[] Read()
    {
        var result = new List<WindowObservation>();
        var seen = new HashSet<(long, uint)>();
        int visited = 0;
        EnumWindows((handle, data) =>
        {
            if (++visited > 2048) return false;
            if (!IsWindowVisible(handle) || IsIconic(handle)) return true;
            if (GetInteger(handle, 14, out int cloaked, sizeof(int)) == 0 && cloaked != 0) return true;
            GetWindowThreadProcessId(handle, out uint pid);
            if (pid == System.Environment.ProcessId && !IncludedOwnedWindows.Contains(handle)) return true;
            long style = GetWindowLongPtr(handle, -16), extended = GetWindowLongPtr(handle, -20);
            if ((extended & 0x20) != 0) return true; // fully input-transparent decoration
            var name = new StringBuilder(128); GetClassNameW(handle, name, name.Capacity);
            if (name.ToString() is "Progman" or "WorkerW" or "Shell_TrayWnd" or "Shell_SecondaryTrayWnd") return true;
            if (GetBounds(handle, 9, out var bounds, Marshal.SizeOf<Rect>()) != 0 && !GetWindowRect(handle, out bounds)) return true;
            var rect = new Rect2I(bounds.Left, bounds.Top, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top);
            if (rect.Size.X <= 0 || rect.Size.Y <= 0) return true;
            var key = ((long)handle, pid); seen.Add(key);
            if (!_identities.ContainsKey(key)) _identities.Add(key, ++_generation);
            bool canSupport = (extended & 0x80) == 0 && (style & 0xC00000) != 0 && rect.Size.X >= 80 && rect.Size.Y >= 40;
            result.Add(new($"window:{pid}:{(long)handle}:{_identities[key]}", rect, CanSupport: canSupport));
            return true;
        }, 0);
        foreach (var key in new List<(long, uint)>(_identities.Keys)) if (!seen.Contains(key)) _identities.Remove(key);
        return result.ToArray();
    }
}
