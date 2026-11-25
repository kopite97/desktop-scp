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
    [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")] private static extern nint GetWindowLongPtr(nint window,int index);
    [DllImport("user32.dll",EntryPoint="SetWindowLongPtrW",SetLastError=true)] private static extern nint SetWindowLongPtr(nint window,int index,nint value);
    [DllImport("user32.dll",SetLastError=true)] private static extern bool SetLayeredWindowAttributes(nint window,uint color,byte alpha,uint flags);
    [DllImport("user32.dll",SetLastError=true)] private static extern bool SetWindowPos(nint window,nint after,int x,int y,int width,int height,uint flags);
    [DllImport("user32.dll")] private static extern nint GetTopWindow(nint parent);
    [DllImport("user32.dll")] private static extern nint GetWindow(nint window,uint command);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(nint window);
    [DllImport("user32.dll",SetLastError=true)] private static extern nint BeginDeferWindowPos(int count);
    [DllImport("user32.dll",SetLastError=true)] private static extern nint DeferWindowPos(nint batch,nint window,nint after,int x,int y,int width,int height,uint flags);
    [DllImport("user32.dll",SetLastError=true)] private static extern bool EndDeferWindowPos(nint batch);

    public static bool StackMatches(System.Collections.Generic.IReadOnlyList<nint> frontToBack)
    {
        if(frontToBack.Count==0)return true;
        var owned=new System.Collections.Generic.HashSet<nint>(frontToBack);
        int index=0;
        for(nint window=GetTopWindow(0);window!=0;window=GetWindow(window,2))
        {
            if(!owned.Contains(window)||!IsWindowVisible(window))continue;
            if(window!=frontToBack[index]||(GetWindowLongPtr(window,-20)&8)==0)return false;
            if(++index==frontToBack.Count)return true;
        }
        return false;
    }

    public static void ApplyStack(System.Collections.Generic.IReadOnlyList<nint> frontToBack)
    {
        nint batch=BeginDeferWindowPos(frontToBack.Count);
        if(batch==0)throw new Win32Exception(Marshal.GetLastWin32Error());
        for(int i=0;i<frontToBack.Count;i++)
        {
            // One deferred operation prevents intermediate per-controller ordering.
            batch=DeferWindowPos(batch,frontToBack[i],i==0?-1:frontToBack[i-1],0,0,0,0,0x213);
            if(batch==0)throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        if(!EndDeferWindowPos(batch))throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public static void PlaceBehind(Window window,Window foreground)
    {
        var handle=(nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,window.GetWindowId());
        var before=(nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,foreground.GetWindowId());
        if(!SetWindowPos(handle,before,0,0,0,0,0x13))throw new Win32Exception(Marshal.GetLastWin32Error()); // no size/move/activate
    }

    public static void RaiseWithoutFocus(Window window)
    {
        var handle=(nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,window.GetWindowId());
        if(!SetWindowPos(handle,-1,0,0,0,0,0x13))throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    // Godot's HTTRANSPARENT flag alone only forwards within the same GUI thread.
    // Decorative native windows must also be excluded from cross-process input routing.
    public static void MakeDecorativeClickThrough(Window window)
    {
        var handle=(nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,window.GetWindowId());
        const int extendedStyle=-20;
        SetWindowLongPtr(handle,extendedStyle,GetWindowLongPtr(handle,extendedStyle)|0x80000|0x20);
        if(!SetLayeredWindowAttributes(handle,0,255,2))throw new Win32Exception(Marshal.GetLastWin32Error());
    }
    [DllImport("user32.dll")] private static extern bool ShowWindow(nint window,int command);
    // Godot does not permit Window.Hide() on its main window. Keep its event loop alive in the tray.
    public static void SetControlVisible(Window window,bool visible)
    {ShowWindow((nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,window.GetWindowId()),visible?5:0);}
    public static bool NativeVisible(Window window)=>IsWindowVisible((nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,window.GetWindowId()));
    public static void SetIconClickThrough(Window window,bool pass)
    {
        var handle=(nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,window.GetWindowId());
        nint style=GetWindowLongPtr(handle,-20),next=pass?(style|0x80000|0x20):(style&~(nint)0x20);
        if(next==style)return;
        SetWindowLongPtr(handle,-20,next);
        if(pass&&!SetLayeredWindowAttributes(handle,0,255,2))throw new Win32Exception(Marshal.GetLastWin32Error());
    }

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

    public static bool RightPressed => (GetAsyncKeyState(0x02) & 0x8000) != 0;

    public static Rect2I GetBounds(Window window)
    {
        var handle = (nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, window.GetWindowId());
        if (!GetWindowRect(handle, out var rect)) throw new Win32Exception(Marshal.GetLastWin32Error());
        return new Rect2I(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public static uint GetDpi(Window window) => GetDpiForWindow(
        (nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, window.GetWindowId()));
}
