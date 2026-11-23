using Godot;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SiteDesktop.Platform.Windows;

public static class WindowsDesktop
{
    [StructLayout(LayoutKind.Sequential)] private struct Point { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll", SetLastError = true)] private static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int virtualKey);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool GetWindowRect(nint window, out Rect rect);
    [DllImport("user32.dll")] private static extern uint GetDpiForWindow(nint window);
    [DllImport("user32.dll")] private static extern int GetSystemMetrics(int index);

    // Godot normalizes its Windows screen coordinates to the virtual desktop's minimum.
    // Win32 cursor/window coordinates retain the primary-screen origin and may be negative.
    public static Vector2I VirtualOrigin => new(GetSystemMetrics(76), GetSystemMetrics(77));
    public static Vector2I ToGodotPosition(Vector2I desktop) => desktop - VirtualOrigin;
    public static Vector2I ToDesktopPosition(Vector2I godot) => godot + VirtualOrigin;

    public static bool TryGetPointer(out Vector2I position, out bool leftPressed)
    {
        bool success = GetCursorPos(out var point);
        position = new Vector2I(point.X, point.Y);
        leftPressed = (GetAsyncKeyState(0x01) & 0x8000) != 0;
        return success;
    }

    public static Rect2I GetBounds(Window window)
    {
        var handle = (nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, window.GetWindowId());
        if (!GetWindowRect(handle, out var rect)) throw new Win32Exception(Marshal.GetLastWin32Error());
        return new Rect2I(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public static uint GetDpi(Window window) => GetDpiForWindow(
        (nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, window.GetWindowId()));
}
