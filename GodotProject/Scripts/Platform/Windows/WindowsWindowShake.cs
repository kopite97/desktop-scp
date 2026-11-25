using Godot;
using System;
using System.Runtime.InteropServices;
using SiteDesktop.Simulation;

namespace SiteDesktop.Platform.Windows;

// One foreground target, bounded translation, no resize/activation/z-order changes.
public sealed class WindowsWindowShake : IDisposable
{
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; public readonly Rect2I Value => new(Left, Top, Right-Left, Bottom-Top); }
    [StructLayout(LayoutKind.Sequential)] private struct GuiInfo { public uint Size, Flags; public nint Active, Focus, Capture, Menu, Move, Caret; public Rect CaretRect; }
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] private static extern bool GetWindowRect(nint window, out Rect rect);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(nint window);
    [DllImport("user32.dll")] private static extern bool IsIconic(nint window);
    [DllImport("user32.dll")] private static extern bool IsZoomed(nint window);
    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window, out uint pid);
    [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")] private static extern nint GetWindowLongPtr(nint window, int index);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(nint window,nint after,int x,int y,int cx,int cy,uint flags);
    [DllImport("user32.dll")] private static extern bool GetGUIThreadInfo(uint thread,ref GuiInfo info);
    private readonly SystemOptions _options;
    private nint _target;
    private uint _pid, _thread;
    private Rect2I _origin, _last;
    private double _age, _step, _cooldown;
    public bool Active => _target != 0;
    public int Moves { get; private set; }
    public int Restores { get; private set; }
    public WindowsWindowShake(SystemOptions options) { _options = options; }
    private static bool Read(nint target, out Rect2I rect)
    { bool ok=GetWindowRect(target,out var r);rect=r.Value;return ok; }
    private bool Valid()
    {
        if (_target==0 || !IsWindowVisible(_target) || IsIconic(_target) || IsZoomed(_target)) return false;
        uint thread=GetWindowThreadProcessId(_target,out uint pid);
        return pid==_pid && thread==_thread && (long)GetWindowLongPtr(_target,-16)!=0;
    }
    private static bool UserMoving(uint thread)
    {
        var info=new GuiInfo{Size=(uint)Marshal.SizeOf<GuiInfo>()};
        return (GetAsyncKeyState(1)&0x8000)!=0 || !GetGUIThreadInfo(thread,ref info) || (info.Flags&0xA)!=0 || info.Move!=0;
    }
    public bool StartForeground()
    {
        if (!OperatingSystem.IsWindows() || !_options.CanAffectWindows || Active || _cooldown>0) return false;
        nint target=GetForegroundWindow();uint thread=GetWindowThreadProcessId(target,out uint pid);
        if (target==0 || pid==0 || pid==System.Environment.ProcessId || UserMoving(thread) ||
            ((long)GetWindowLongPtr(target,-16)&0xC00000)!=0xC00000 || !IsWindowVisible(target) || IsIconic(target) || IsZoomed(target) || !Read(target,out var rect)) return false;
        _target=target;_pid=pid;_thread=thread;_origin=_last=rect;_age=_step=0;_cooldown=20;
        return true;
    }
    public void Tick(double delta)
    {
        _cooldown=Math.Max(0,_cooldown-Math.Clamp(delta,0,.1));
        if (!Active) return;
        if (!Valid() || !Read(_target,out var rect) || rect!=_last) { _target=0; return; }
        // Let user movement win. Never undo a user drag or resize.
        if (UserMoving(_thread)) { _target=0; return; }
        if (!_options.CanAffectWindows || GetForegroundWindow()!=_target) { Cancel(); return; }
        _age+=Math.Clamp(delta,0,.1);_step+=delta;
        if (_age>=1.2) { Cancel(); return; }
        if (_step<.05) return;_step=0;
        int x=_origin.Position.X+(((int)(_age/.05)&1)==0?6:-6);
        if (SetWindowPos(_target,0,x,_origin.Position.Y,0,0,0x215))
        { _last=new(x,_origin.Position.Y,_origin.Size.X,_origin.Size.Y);Moves++; }
        else Cancel();
    }
    public void Cancel()
    {
        if (Active && Valid() && Read(_target,out var rect) && rect==_last && !UserMoving(_thread) &&
            SetWindowPos(_target,0,_origin.Position.X,_origin.Position.Y,0,0,0x215)) Restores++;
        _target=0;
    }
    public void Dispose()=>Cancel();
}
