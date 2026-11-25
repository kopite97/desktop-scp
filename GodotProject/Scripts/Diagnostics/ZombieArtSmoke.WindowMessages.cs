using Godot;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SiteDesktop.Diagnostics;

public partial class ZombieArtSmoke
{
    // Observe only the diagnostic's own Godot HWND on the same UI thread.
    // Unlike a viewport image, native messages expose redundant SetWindowRgn redraws.
    private sealed class WindowRedrawProbe : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate nint SubclassProc(nint window,uint message,nuint wp,nint lp,nuint id,nuint data);
        [DllImport("comctl32.dll",SetLastError=true)] private static extern bool SetWindowSubclass(nint window,SubclassProc proc,nuint id,nuint data);
        [DllImport("comctl32.dll")] private static extern bool RemoveWindowSubclass(nint window,SubclassProc proc,nuint id);
        [DllImport("comctl32.dll")] private static extern nint DefSubclassProc(nint window,uint message,nuint wp,nint lp);
        private readonly nint _window;
        private readonly SubclassProc _proc;
        public int PositionChanges { get; private set; }
        public int BackgroundErases { get; private set; }
        public int NonClientPaints { get; private set; }
        public WindowRedrawProbe(Window window)
        {
            _window=(nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,window.GetWindowId());
            _proc=Observe;
            if(!SetWindowSubclass(_window,_proc,14001,0))throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        private nint Observe(nint window,uint message,nuint wp,nint lp,nuint id,nuint data)
        {
            if(message==0x47)PositionChanges++;
            if(message==0x14)BackgroundErases++;
            if(message==0x85)NonClientPaints++;
            return DefSubclassProc(window,message,wp,lp);
        }
        public void Dispose(){RemoveWindowSubclass(_window,_proc,14001);GC.KeepAlive(_proc);}
    }
}
