using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Platforms;
using SiteDesktop.Overlay;
using System;
using System.Diagnostics;
using System.Linq;

namespace SiteDesktop.Platform.Windows;
public sealed class WindowsDesktopEnvironmentSource : IDesktopEnvironmentSource
{
    public WindowsWindowObserver Observer { get; }=new();
    public DesktopEnvironmentSnapshot Current { get; private set; }
    public long ObservationCount { get; private set; }
    private readonly Stopwatch _clock=Stopwatch.StartNew();
    private double _time,_windowAge=1,_screenAge=1;
    private ulong _frame=ulong.MaxValue;
    private readonly bool _native;
    public WindowsDesktopEnvironmentSource()
    {
        _native=OperatingSystem.IsWindows()&&DisplayServer.GetName()!="headless";
        var all=_native?OverlayController.ReadScreens():new[]{DesktopPlacement.Select()};
        var selected=Select(all);
        Current=new(0,0,selected,all,Array.Empty<WindowObservation>(),null,false,false);
        Capture(0,true);
    }
    private static DesktopScreen[] Select(DesktopScreen[] all)
    {
        if(DesktopPlacement.RequestedMonitor=="")return all;
        try{return new[]{DesktopPlacement.Select()};}catch(InvalidOperationException){return all.Take(1).ToArray();}
    }
    public DesktopEnvironmentSnapshot Capture(double delta,bool force=false)
    {
        ulong frame=Engine.GetProcessFrames();if(frame==_frame)return Current;_frame=frame;
        _time=_clock.Elapsed.TotalSeconds;_windowAge+=delta;_screenAge+=delta;
        var all=Current.AllScreens;var selected=Current.Screens;var windows=Current.Windows;double observed=Current.ObservedAt;
        if(_native&&(force||_screenAge>=.5)){_screenAge=0;var read=OverlayController.ReadScreens();all=read;selected=Select(read);}
        if(_native&&(force||_windowAge>=.05)){_windowAge=0;windows=Observer.Read();observed=_time;ObservationCount++;}
        bool changed=!all.SequenceEqual(Current.AllScreens)||!selected.SequenceEqual(Current.Screens)||!windows.SequenceEqual(Current.Windows);
        // Preserve collection identity when unchanged, including activation within the frame.
        if(selected.SequenceEqual(Current.Screens))selected=Current.Screens;if(all.SequenceEqual(Current.AllScreens))all=Current.AllScreens;
        Vector2? pointer=null;bool left=false,right=false;
        if(_native&&WindowsDesktop.TryGetPointer(out var p,out left)){pointer=p;right=WindowsDesktop.RightPressed;}
        Current=new(Current.Version+(changed?1:0),observed,selected,all,windows,pointer,left,right);return Current;
    }
}
