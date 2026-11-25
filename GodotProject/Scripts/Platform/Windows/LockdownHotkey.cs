using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SiteDesktop.Platform.Windows;

public sealed class LockdownHotkey : IDisposable
{
    [StructLayout(LayoutKind.Sequential)] private struct Message { public nint Window; public uint Id; public nuint WParam; public nint LParam; public uint Time; public int X, Y; public uint Private; }
    [DllImport("user32.dll")] private static extern bool RegisterHotKey(nint window, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(nint window, int id);
    [DllImport("user32.dll")] private static extern int GetMessage(out Message msg, nint window, uint min, uint max);
    [DllImport("user32.dll")] private static extern bool PeekMessage(out Message msg, nint window, uint min, uint max, uint remove);
    [DllImport("user32.dll")] private static extern bool PostThreadMessage(uint thread, uint msg, nuint w, nint l);
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    private Thread? _thread;
    private uint _id;
    private int _pending;
    private volatile bool _stop;
    public bool Registered { get; private set; }
    public bool Running => _thread?.IsAlive == true;
    public void Start()
    {
        if (_thread != null || !OperatingSystem.IsWindows()) return;
        _thread = new Thread(Run) { IsBackground = true, Name = "Site lockdown hotkey" }; _thread.Start();
    }
    private void Run()
    {
        _id = GetCurrentThreadId(); PeekMessage(out _, 0, 0, 0, 0);
        try
        {
            Registered = RegisterHotKey(0, 1, 0x4006, 0x4C);
            if (!Registered) return;
            while (!_stop && GetMessage(out var msg, 0, 0, 0) > 0)
                if (msg.Id == 0x312 && msg.WParam == 1) Interlocked.Increment(ref _pending);
        }
        finally { if (Registered) UnregisterHotKey(0, 1); Registered = false; }
    }
    public bool Consume() => (Interlocked.Exchange(ref _pending, 0) & 1) != 0;
    public void Dispose()
    {
        _stop = true;
        if (_id != 0) PostThreadMessage(_id, 0x12, 0, 0);
        _thread?.Join(500); _pending = 0;
    }
}
