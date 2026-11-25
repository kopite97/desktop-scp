using Godot;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using SiteDesktop.Platform.Windows;

namespace SiteDesktop.Diagnostics;

// Bounded diagnostics only: target our separate receiver process; retain no input content.
internal sealed class NativeInputFixture : IDisposable
{
    [StructLayout(LayoutKind.Sequential)] private struct Point { public int X,Y; }
    [StructLayout(LayoutKind.Sequential)] private struct MouseData { public int X,Y; public uint Data,Flags,Time; public nuint Extra; }
    [StructLayout(LayoutKind.Sequential)] private struct KeyData { public ushort Key,Scan; public uint Flags,Time; public nuint Extra; }
    [StructLayout(LayoutKind.Explicit)] private struct InputUnion { [FieldOffset(0)] public MouseData Mouse; [FieldOffset(0)] public KeyData Key; }
    [StructLayout(LayoutKind.Sequential)] private struct NativeInput { public uint Type; public InputUnion Data; }
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x,int y);
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] private static extern nint WindowFromPoint(Point point);
    [DllImport("user32.dll")] private static extern bool ClientToScreen(nint window,ref Point point);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(nint window);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window,out uint pid);
    [DllImport("user32.dll",SetLastError=true)] private static extern uint SendInput(uint count,NativeInput[] inputs,int size);
    private readonly int _pid;
    private readonly Point _pointer;
    private readonly nint _foreground;
    private readonly string _file;
    private uint _heldRelease;
    public NativeInputFixture(string directory,bool topmost=false,bool decorated=false)
    {
        GetCursorPos(out _pointer);_foreground=GetForegroundWindow();_file=Path.Combine(directory,"receiver.json");
        _pid=OS.CreateProcess(OS.GetExecutablePath(),new[]{"--path",ProjectSettings.GlobalizePath("res://"),"--scene","res://Scenes/Diagnostics/InputProbe.tscn","--max-fps","60","--log-file",Path.Combine(directory,"receiver.log"),"--","--receiver-output",_file,"--monitor-id",SiteDesktop.Desktop.DesktopPlacement.RequestedMonitor,"--receiver-on-top",topmost?"true":"false","--receiver-decorated",decorated?"true":"false"});
        if(_pid<=0)throw new InvalidOperationException("Could not start owned input receiver.");
    }
    public static long WindowAt(Vector2 point)=>(long)WindowFromPoint(new Point{X=(int)point.X,Y=(int)point.Y});
    public bool Ready=>File.Exists(_file);
    public nint Handle
    {get{using var doc=JsonDocument.Parse(File.ReadAllText(_file));var handle=(nint)doc.RootElement.GetProperty("handle").GetInt64();GetWindowThreadProcessId(handle,out uint pid);if(pid!=_pid)throw new InvalidOperationException("Receiver ownership changed.");return handle;}}
    public (int Down,int Up,int Keys) Counts()
    {using var doc=JsonDocument.Parse(File.ReadAllText(_file));var r=doc.RootElement;return(r.GetProperty("down").GetInt32(),r.GetProperty("up").GetInt32(),r.GetProperty("keys").GetInt32());}
    public bool Focus()
    {
        using var doc=JsonDocument.Parse(File.ReadAllText(_file));var handle=(nint)doc.RootElement.GetProperty("handle").GetInt64();
        GetWindowThreadProcessId(handle,out uint pid);
        if(pid!=_pid)throw new InvalidOperationException("Receiver ownership changed.");
        SetForegroundWindow(handle);
        if(GetForegroundWindow()==handle)return true;
        // A real click on our verified receiver can activate it when Windows rejects background focus requests.
        var point=new Point{X=32,Y=80};
        if(!ClientToScreen(handle,ref point))return false;
        GetWindowThreadProcessId(WindowFromPoint(point),out uint targetPid);
        if(targetPid!=_pid)return false;
        if(!SetCursorPos(point.X,point.Y))return false;
        GetWindowThreadProcessId(WindowFromPoint(point),out targetPid);
        if(targetPid!=_pid)return false;
        Send(new NativeInput{Data=new InputUnion{Mouse=new MouseData{Flags=2}}},new NativeInput{Data=new InputUnion{Mouse=new MouseData{Flags=4}}});
        return GetForegroundWindow()==handle;
    }
    public bool FocusWithoutInput(){var handle=Handle;SetForegroundWindow(handle);return GetForegroundWindow()==handle;}
    private void CheckFocus()
    {GetWindowThreadProcessId(GetForegroundWindow(),out uint pid);if(pid!=_pid)throw new InvalidOperationException("Test receiver lost foreground; input cancelled.");}
    public void Move(Vector2 point){CheckFocus();if(!SetCursorPos((int)point.X,(int)point.Y))throw new Win32Exception();}
    public void Click()
    {CheckFocus();Send(new NativeInput{Data=new InputUnion{Mouse=new MouseData{Flags=2}}},new NativeInput{Data=new InputUnion{Mouse=new MouseData{Flags=4}}});}
    public void KeyActivity()
    {CheckFocus();Send(new NativeInput{Type=1,Data=new InputUnion{Key=new KeyData{Key=0x87}}},new NativeInput{Type=1,Data=new InputUnion{Key=new KeyData{Key=0x87,Flags=2}}});}
    public void LockdownChord()
    {
        CheckFocus();
        static NativeInput Key(ushort key,bool up=false)=>new(){Type=1,Data=new InputUnion{Key=new KeyData{Key=key,Flags=up?2u:0}}};
        try {Send(Key(0x11),Key(0x10),Key(0x4c),Key(0x4c,true));}
        finally {Send(Key(0x4c,true),Key(0x10,true),Key(0x11,true));}
    }
    public void Press(bool right=false)
    {CheckFocus();_heldRelease=right?16u:4u;Send(new NativeInput{Data=new InputUnion{Mouse=new MouseData{Flags=right?8u:2u}}});}
    public void Release()
    {if(_heldRelease==0)return;Send(new NativeInput{Data=new InputUnion{Mouse=new MouseData{Flags=_heldRelease}}});_heldRelease=0;}
    private static void Send(params NativeInput[] inputs)
    {if(SendInput((uint)inputs.Length,inputs,Marshal.SizeOf<NativeInput>())!=inputs.Length)throw new Win32Exception(Marshal.GetLastWin32Error());}
    public void Dispose()
    {
        Release();
        if(OS.IsProcessRunning(_pid))OS.Kill(_pid);
        SetCursorPos(_pointer.X,_pointer.Y);
        if(_foreground!=0)SetForegroundWindow(_foreground);
    }
}
