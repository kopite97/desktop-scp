using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace SiteDesktop.Platform.Windows;

// Activity only: the native key-data pointer is never decoded, copied or logged.
public sealed class KeyboardActivityMonitor : IDisposable
{
    private delegate nint HookCallback(int code,nint message,nint data);
    [DllImport("user32.dll",SetLastError=true)] private static extern nint SetWindowsHookExW(int id,HookCallback callback,nint module,uint thread);
    [DllImport("user32.dll",SetLastError=true)] private static extern bool UnhookWindowsHookEx(nint hook);
    [DllImport("user32.dll")] private static extern nint CallNextHookEx(nint hook,int code,nint message,nint data);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] private static extern nint GetModuleHandleW(string? module);
    private readonly HookCallback _callback;
    private nint _hook;
    private int _pending;
    public bool Running => _hook!=0;
    public KeyboardActivityMonitor() => _callback=OnActivity;
    public void Start()
    {
        if(Running||!OperatingSystem.IsWindows())return;
        Interlocked.Exchange(ref _pending,0);
        _hook=SetWindowsHookExW(13,_callback,GetModuleHandleW(null),0);
        if(_hook==0)throw new Win32Exception(Marshal.GetLastWin32Error());
    }
    private nint OnActivity(int code,nint message,nint data)
    {
        if(code>=0 && (message==0x100||message==0x104))Interlocked.Exchange(ref _pending,1);
        return CallNextHookEx(_hook,code,message,data);
    }
    public bool Consume() => Interlocked.Exchange(ref _pending,0)!=0;
    public void Dispose()
    {
        if(_hook!=0)
        {
            if(!UnhookWindowsHookEx(_hook))throw new Win32Exception(Marshal.GetLastWin32Error());
            _hook=0;
        }
        Interlocked.Exchange(ref _pending,0);
    }
}
