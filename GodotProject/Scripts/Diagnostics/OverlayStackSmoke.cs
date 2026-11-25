using Godot;
using SiteDesktop.App;
using SiteDesktop.Companions;
using SiteDesktop.Desktop;
using SiteDesktop.Platform.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;

public partial class OverlayStackSmoke : Node
{
    [DllImport("user32.dll")] private static extern nint GetTopWindow(nint parent);
    [DllImport("user32.dll")] private static extern nint GetWindow(nint window,uint command);
    private Main _main=null!;
    private Window? _background;
    private string _output="";
    private readonly List<object> _trace=new();
    private readonly List<string> _checks=new();
    private Rect2I _capture;
    private Vector2 _center;
    private int _frame;
    private static long Handle(Window w)=>(long)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,w.GetWindowId());
    private bool Front999()
    {
        long a=Handle(_main.Companion.BodyWindow!),b=Handle(_main.Plague!.DoctorWindow!);
        for(nint h=GetTopWindow(0);h!=0;h=GetWindow(h,2)){if((long)h==a)return true;if((long)h==b)return false;}
        throw new InvalidOperationException("Actor HWND absent from native stack.");
    }
    private void Sample(string phase,string point)=>_trace.Add(new{ms=Time.GetTicksMsec(),frame=_frame,phase,point,front=Front999()?"999":"049"});
    private void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);_checks.Add(message);GD.Print("STACK_CHECK_PASS "+message);}
    private async Task Frames(int count){for(int i=0;i<count;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private void Capture(string phase)
    {
        using var image=DisplayServer.ScreenGetImageRect(_capture);
        image.SavePng(Path.Combine(_output,"frames",$"{_frame:D5}-{phase}.png"));
    }
    private async Task Movement(string phase,double seconds,float speed,float range)
    {
        var c=_main.Companion;var p=_main.Plague!;
        c.State.EndGesture();c.State.Physics.Place(_center);p.State.Doctor.Place(_center+new Vector2(22,0));
        if(speed>0)c.State.BeginDrag(_center);
        c.Publish();p.Publish();await Frames(4);
        int batches=_main.Stack.AppliedBatches;bool expected=speed>0;
        ulong started=Time.GetTicksMsec();double observation=0;
        while((Time.GetTicksMsec()-started)/1000d<seconds)
        {
            double t=(Time.GetTicksMsec()-started)/1000d;
            if(speed>0)c.Advance(1d/60,_center+new Vector2((float)Math.Sin(t*speed)*range,0),right:true);
            else c.Publish();
            p.Publish();
            if((observation+=1d/60)>=.05)
            {
                observation=0;c.RefreshPlatforms();Sample(phase,"after-999-observe");p.Refresh();Sample(phase,"after-049-observe");
            }
            await Frames(1);_frame++;Sample(phase,"present");
            if(Front999()!=expected)throw new InvalidOperationException(phase+" unexpected actor order");
            if(_frame%2==0)Capture(phase);
        }
        {Require(_main.Stack.AppliedBatches==batches,phase+" stable stack has zero repeated native batches");Require(Front999()==expected,phase+" actual native order stable");}
        c.State.EndGesture();c.Publish();
    }
    private async Task Lifecycle()
    {
        var c=_main.Companion;var p=_main.Plague!;
        for(int i=0;i<5;i++)
        {
            c.State.BeginDrag(c.State.Foot);c.Publish();await Frames(3);Require(Front999(),"drag start priority "+i);
            c.State.EndGesture();c.Publish();await Frames(3);Require(!Front999(),"drag release base order "+i);
        }
        p.State.BeginDoctorDrag(p.State.Doctor.Body.Foot);p.Publish();await Frames(3);Require(!Front999(),"doctor drag priority");p.State.EndDrag();p.Publish();
        int id=p.State.SpawnAt(_center)!.Value;p.Publish();await Frames(3);
        p.Objects.State.BeginObjectDrag(id,_center);p.Publish();await Frames(3);
        Require(WindowsDesktop.StackMatches(new[]{(nint)Handle(p.ObjectWindows[id]),(nint)Handle(p.DoctorWindow!),(nint)Handle(c.BodyWindow!)}),"data drag promoted above both actors");
        p.Objects.State.EndDrag();p.State.Collect(id);p.Publish();await Frames(3);Require(!p.ObjectWindows.ContainsKey(id),"removed object leaves stack");
        c.State.BeginDrag(c.State.Foot);c.Publish();await Frames(3);c.SetVisible(false);p.SetVisible(false);await Frames(3);
        Require(!c.BodyWindow!.Visible&&!p.DoctorWindow!.Visible&&c.State.Gesture!=CompanionGesture.Drag,"hide cancels drag and unregisters native islands");
        c.SetVisible(true);p.SetVisible(true);await Frames(5);Require(!Front999(),"restored handles regain base order");
        WindowsDesktop.RaiseWithoutFocus(c.BodyWindow);await Frames(10);Require(!Front999(),"actual external order drift repaired");
        c.ResetPositions();c.State.TypingActivity();c.Publish();await Frames(3);
        var reaction=c.EffectWindows.First(w=>w.Effect.Kind==CompanionEffectKind.Reaction);
        Require(WindowsDesktop.StackMatches(new[]{(nint)Handle(reaction),(nint)Handle(p.DoctorWindow!),(nint)Handle(c.BodyWindow!)}),"reaction above base actors");
        p.State.BeginDoctorDrag(p.State.Doctor.Body.Foot);p.Publish();await Frames(3);
        Require(WindowsDesktop.StackMatches(new[]{(nint)Handle(p.DoctorWindow!),(nint)Handle(reaction),(nint)Handle(c.BodyWindow!)}),"other actor drag stays ahead of reaction");
        p.State.EndDrag();p.Publish();c.State.EndGesture();
        for(int i=0;i<180;i++)c.Advance(.02,c.State.Foot);
        await Frames(3);Require(c.EffectWindows.All(w=>w.Effect.Kind!=CompanionEffectKind.Reaction),"expired reaction removed from stack");
    }
    public override async void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();
            for(int i=0;i+1<args.Length;i++)if(args[i]=="--output")_output=args[++i];
            if(_output==""||File.Exists(Path.Combine(_output,"results.json")))throw new InvalidOperationException("New evidence required.");
            Directory.CreateDirectory(Path.Combine(_output,"frames"));
            _main=GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();_main.PersistScpSettings=false;_main.EnableSystemOptions=false;_main.EnableStatue=false;_main.UseDesktopIcons=false;AddChild(_main);
            _main.Companion.AutomaticInput=false;_main.Plague!.Automatic=false;_main.Plague.State.AutoSpawn=false;
            foreach(int id in _main.Plague.State.Objects.Keys.ToArray())_main.Plague.State.Collect(id);
            var area=DesktopPlacement.Select().Usable;_center=area.Position+new Vector2(1150,650);
            _capture=new(WindowsDesktop.ToGodotPosition((Vector2I)_center-new Vector2I(220,170)),new(440,230));
            _background=new Window{Title="Overlay stack owned background",Visible=false,ForceNative=true,Transient=false,Borderless=true,AlwaysOnTop=false,Unfocusable=true,Position=WindowsDesktop.ToGodotPosition(area.Position),Size=area.Size};
            _background.AddChild(new ColorRect{Color=new Color("263440"),Size=area.Size});AddChild(_background);_background.Show();
            await Frames(10);_main.Companion.RefreshPlatforms();_main.Plague.Refresh();
            File.WriteAllText(Path.Combine(_output,"monitor.json"),JsonSerializer.Serialize(new{monitor=DesktopPlacement.RequestedMonitor,workArea=area.ToString(),dpi=WindowsDesktop.GetDpi(_main.Companion.BodyWindow!)}));
            if(args.Contains("--native-input"))await NativeOverlap();
            else
            {
                await Movement("stationary",10,0,0);
                await Movement("slow-drag",10,2,65);
                await Movement("fast-drag",10,12,80);
                await Lifecycle();
            }
            GD.Print("STACK_CAPTURE_COMPLETE");
            File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new{passed=true,checks=_checks,frames=_frame,nativeInput=args.Contains("--native-input"),batches=_main.Stack.AppliedBatches}));
            GD.Print("OVERLAY_STACK_PASS");_main.RequestExit();
        }
        catch(Exception ex){GD.PushError("OVERLAY_STACK_FAILED "+ex);_main?.Companion.Shutdown();_main?.Plague?.Shutdown();GetTree().Quit(1);}
        finally
        {
            if(_output!="")File.WriteAllText(Path.Combine(_output,"stack-trace.json"),JsonSerializer.Serialize(_trace));
            if(_background!=null&&IsInstanceValid(_background)){_background.Hide();_background.QueueFree();}
        }
    }
}
