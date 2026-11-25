using Godot;
using SiteDesktop.App;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;

public partial class SystemSmoke : Node
{
    private Main? _main;
    private string _output="";
    private readonly List<string> _checks=new();
    private object? _metricsEvidence;
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int L,T,R,B; public readonly Rect2I Value=>new(L,T,R-L,B-T); }
    [DllImport("user32.dll")] private static extern bool GetWindowRect(nint hwnd,out Rect rect);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(nint hwnd,nint after,int x,int y,int w,int h,uint flags);
    private static Rect2I Bounds(nint h){if(!GetWindowRect(h,out var r))throw new InvalidOperationException("Owned receiver missing");return r.Value;}
    private void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);_checks.Add(name);GD.Print("SYSTEM_CHECK "+name);}
    private async Task Frames(int n){for(int i=0;i<n;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private void Logic()
    {
        var s=new SystemOptions();Check(!s.StrongEffects&&!s.Locked&&!s.CanAffectWindows,"Fresh session defaults strong effects OFF");
        s.Accept(new(79,84,69,false),0);Check(!s.Hot&&!s.Noisy,"Below thresholds leaves reactions inactive");
        s.Accept(new(80,84,70,false),0);Check(s.Hot&&s.Noisy,"Threshold boundaries enable heat and noise");
        s.Accept(new(71,74,66,false),0);Check(s.Hot&&s.Noisy,"Hysteresis avoids threshold flicker");
        s.Accept(new(69,74,64,false),0);Check(!s.Hot&&!s.Noisy,"Low readings release reactions");
        s.Accept(new(20,85,100,true),0);Check(s.Hot&&!s.Noisy,"RAM independently enables heat; mute suppresses noise");
        s.Accept(new(float.NaN,-1,101,false),0);Check(!s.Hot&&!s.Noisy&&s.Sample.Cpu==null,"Invalid metrics become unavailable");
        s.Accept(new(90,90,90,false),0);for(int i=0;i<51;i++)s.Tick(.1);Check(!s.Hot&&!s.Noisy,"Stale readings expire after five seconds");
        s.SetStrong(true);Check(s.CanAffectWindows,"Explicit option enables effect requests");
        s.SetLocked(true);Check(!s.CanAffectWindows&&!s.Accept(new(90,90,90,false),0),"Lockdown rejects effects and late metrics");
        s.SetLocked(false);Check(!s.Accept(new(90,90,90,false),0)&&!s.Hot,"Resume rejects old generation without catch-up");
        s.Shutdown();Check(!s.CanAffectWindows&&!s.Accept(new(90,90,90,false),s.Generation),"Shutdown rejects subsequent state updates");
    }
    public override async void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();for(int i=0;i+1<args.Length;i++)if(args[i]=="--output")_output=args[++i];
            if(_output==""||File.Exists(Path.Combine(_output,"results.json")))throw new InvalidOperationException("New evidence directory required");
            Directory.CreateDirectory(_output);Logic();
            _main=GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();_main.PersistScpSettings=false;AddChild(_main);
            var sys=_main.Systems!;sys.PlayAlarm=false;sys.AutomaticMetrics=false;
            _main.Companion.AutomaticInput=false;_main.Plague!.Automatic=false;_main.Plague.State.AutoSpawn=false;_main.Statue!.Automatic=false;
            bool native=DisplayServer.GetName()!="headless";
            if(native) await TrayResidency(sys);
            sys.Accept(new(90,90,90,false),sys.State.Generation);float stress=_main.Statue.State.Stress.Value;sys.ApplyReactions(.1);
            Check(_main.Companion.State.Overheated&&_main.Companion.State.Physics.SpeedScale==.6f&&_main.Plague.State.Doctor.SpeedScale==1.5f&&_main.Plague.State.TreatmentRate==1.5f,"Product metric input updates actual companion speed, shape and surgery rate");
            Check(_main.Statue.State.Stress.Value>stress,"Product noise increases common statue stress");
            if(native)
            {
                _main.Companion.Publish();await Frames(3);
                var hotPolygon=_main.Companion.BodyWindow!.MousePassthroughPolygon;
                using(var hotImage=_main.Companion.BodyWindow.GetTexture().GetImage())hotImage.SavePng(Path.Combine(_output,"companion-hot.png"));
                sys.Accept(new(20,20,20,false),sys.State.Generation);_main.Companion.Publish();
                var coolPolygon=_main.Companion.BodyWindow.MousePassthroughPolygon;
                Check(hotPolygon.Length==coolPolygon.Length&&hotPolygon.Zip(coolPolygon).All(p=>Math.Abs(p.First.Y-(112+(p.Second.Y-112)*.75))<.01),"Flattened sprite and input contour share the same transform");
                await Frames(3);
                using var shot=_main.Companion.BodyWindow.GetTexture().GetImage();shot.SavePng(Path.Combine(_output,"companion.png"));
                sys.AutomaticMetrics=true;
                for(int i=0;i<240&&sys.Samples<4;i++)await Frames(1);
                Check(sys.State.Sample.Cpu is >=0 and <=100&&sys.State.Sample.Ram is >=0 and <=100,"Real aggregate Windows CPU and RAM reach product state");
                Check(sys.State.Sample.Volume is >=0 and <=100,"Real default-output volume reaches product state");
                _metricsEvidence=new{sample=sys.State.Sample,samples=sys.Samples,maxReadMilliseconds=sys.MaxSampleMilliseconds};sys.AutomaticMetrics=false;
                for(int i=0;i<180&&sys.Sampling;i++)await Frames(1);
                Check(sys.Tray?.Available==true&&NativeMenu.GetItemCount(sys.Tray.Menu)==3,"Native tray indicator and three-item menu created");
                Check(sys.Hotkey.Registered,"Real Ctrl+Shift+L global hotkey registered");
                _main.ShowControls();
                await Frames(16);
                var content=_main.GetNode<VBoxContainer>("Scroll/Content");Check(content.Position.Y+content.Size.Y<=_main.GetWindow().Size.Y,"All product controls fit inside the LG control window");
                using(var panel=_main.GetWindow().GetTexture().GetImage())panel.SavePng(Path.Combine(_output,"controls.png"));
            }
            sys.Accept(new(20,20,20,false),sys.State.Generation);
            var before=_main.Companion.State.Foot;double clock=_main.Plague.State.Clock;stress=_main.Statue.State.Stress.Value;
            _main.GetNode<Button>("Scroll/Content/Visibility").EmitSignal(Button.SignalName.Pressed);
            Check(sys.State.Locked&&!_main.Companion.State.Visible&&!_main.Plague.State.Visible&&!_main.Statue.State.Visible&&_main.Icons!.Paused,"UI button locks all actors and icon service through shared command");
            for(int i=0;i<10;i++){_main.Companion.Advance(.1,before);_main.Plague.Advance(.1);sys.ApplyReactions(.1);}
            if(native)
            {
                await Frames(24);
                Check(sys.Cue?.Visible==true&&sys.Cue.Size==new Vector2I(440,112),"Lockdown shows a compact readable banner while actors are already hidden");
                using(var cue=sys.Cue!.GetTexture().GetImage())cue.SavePng(Path.Combine(_output,"lockdown-banner.png"));
            }
            await ToSignal(GetTree().CreateTimer(SiteDesktop.Overlay.SystemCueWindow.LockdownCueSeconds+.1),SceneTreeTimer.SignalName.Timeout);
            Check(_main.Companion.State.Foot==before&&_main.Plague.State.Clock==clock&&_main.Statue.State.Stress.Value==stress,"Lockdown freezes position, simulation time and stress");
            if(native)
            {
                Check(!_main.Companion.BodyWindow!.Visible&&!_main.Plague.DoctorWindow!.Visible&&!_main.Statue.Actor!.Visible&&sys.Cue?.Visible==false&&!WindowsDesktop.NativeVisible(_main.GetWindow()),"Lockdown removes native actor, control panel and shutter input surfaces");
                sys.Tray!.ToggleCallback.Call(default(Variant));
                Check(!sys.State.Locked&&_main.Companion.State.Visible&&_main.Statue.State.Visible&&!_main.Icons!.Paused,"Registered tray callback resumes the same application state");
                Check(NativeMenu.GetItemText(sys.Tray.Menu,0)=="Lockdown","Tray menu reflects resumed state");
                Check(!WindowsDesktop.NativeVisible(_main.GetWindow()),"Tray resume does not reopen hidden controls");
            }
            else sys.ToggleLockdown();
            Check(_main.Companion.State.Foot==before&&_main.Plague.State.Clock==clock,"Resume preserves existing state without elapsed-time burst");
            bool input=args.Contains("--native-input");if(input)await NativeInput(sys);
            sys.SetStrong(true);sys.Shutdown();await Frames(3);
            Check(sys.State.Stopped&&!sys.Shake.Active&&!sys.Hotkey.Running&&!sys.Hotkey.Registered&&sys.Tray?.Available!=true&&!sys.Sampling,"Shutdown releases effect, hotkey, tray and sampling owner");
            if(native)
            {
                using var restart=new LockdownHotkey();restart.Start();await Frames(6);
                Check(restart.Registered,"Hotkey can register again after shutdown");
            }
            if(native)
            {
                _main.QueueFree();await Frames(3);
                _main=GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();_main.PersistScpSettings=false;
                _main.TrayFactory=(toggle,controls,exit)=>new SiteTray(toggle,controls,exit,(_,_,_)=>-1);
                AddChild(_main);await Frames(3);
                Check(!_main.CanHideControls&&WindowsDesktop.NativeVisible(_main.GetWindow()),"Failed tray registration restores accessible controls");
                _main.GetWindow().EmitSignal(Window.SignalName.CloseRequested);
                Check(WindowsDesktop.NativeVisible(_main.GetWindow())&&_main.GetNode<Button>("Scroll/Content/Exit").Text=="Exit","Failed tray registration keeps X from stranding the app and retains Exit");
            }
            File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new{passed=true,native,nativeInput=input,checks=_checks,metrics=_metricsEvidence},new JsonSerializerOptions{WriteIndented=true}));
            GD.Print("SYSTEM_PASS");
            if(native)_main.GetNode<Button>("Scroll/Content/Exit").EmitSignal(Button.SignalName.Pressed);
            else _main.RequestExit();
        }
        catch(Exception ex){GD.PushError("SYSTEM_FAILED "+ex);_main?.RequestExit();GetTree().Quit(1);}
    }
    private async Task TrayResidency(SystemController sys)
    {
        Check(_main!.CanHideControls&&!WindowsDesktop.NativeVisible(_main.GetWindow())&&!sys.State.Locked,"Startup hides controls while leaving the product active");
        _main.Plague!.Automatic=true;double clock=_main.Plague.State.Clock;
        await Frames(20);
        Check(_main.Plague.State.Clock>clock&&_main.Companion.BodyWindow!.Visible&&_main.Plague.DoctorWindow!.Visible&&_main.Statue!.Actor!.Visible,"Native actor windows and simulation advance with hidden controls");
        NativeMenu.GetItemCallback(sys.Tray!.Menu,1).Call(default(Variant));
        Check(WindowsDesktop.NativeVisible(_main.GetWindow())&&!sys.State.Locked,"Registered Show controls callback opens active session");
        clock=_main.Plague.State.Clock;
        _main.GetWindow().EmitSignal(Window.SignalName.CloseRequested);await Frames(20);
        Check(!WindowsDesktop.NativeVisible(_main.GetWindow())&&!sys.State.Locked&&_main.Plague.State.Clock>clock,"Close-request signal hides controls without pausing simulation");
        _main.ShowControls();_main.GetNode<Button>("Scroll/Content/Exit").EmitSignal(Button.SignalName.Pressed);
        Check(!WindowsDesktop.NativeVisible(_main.GetWindow())&&!sys.State.Stopped,"Control button hides instead of exiting when tray is available");
        sys.SetLocked(true);_main.ShowControls();
        Check(WindowsDesktop.NativeVisible(_main.GetWindow())&&sys.State.Locked,"Opening controls during lockdown preserves pause");
        sys.SetLocked(false);
        Check(WindowsDesktop.NativeVisible(_main.GetWindow())&&!sys.State.Locked,"Resume preserves explicitly opened controls");
        _main.HideControls();_main.Plague.Automatic=false;
    }
    private async Task NativeInput(SystemController sys)
    {
        using var receiver=new NativeInputFixture(_output,decorated:true);
        for(int i=0;i<600&&!receiver.Ready;i++)await Frames(1);Check(receiver.Ready,"Owned decorated receiver started on LG");
        bool focus=false;for(int i=0;i<60&&!focus;i++){focus=receiver.Focus();await Frames(1);}Check(focus,"Owned receiver has foreground before input");
        var h=receiver.Handle;var origin=Bounds(h);
        sys.SetStrong(false);Check(!sys.Shake.StartForeground()&&Bounds(h)==origin,"Default OFF never moves owned external window");
        sys.SetStrong(true);Check(sys.Shake.StartForeground(),"Explicit ON accepts eligible owned foreground target");
        sys.Shake.Tick(.06);Check(Bounds(h)!=origin,"Native effect actually moves the owned window");
        sys.SetStrong(false);Check(!sys.Shake.Active&&Bounds(h)==origin,"Turning OFF immediately cancels and restores original bounds");
        for(int i=0;i<210;i++)sys.Shake.Tick(.1);
        sys.SetStrong(true);Check(sys.Shake.StartForeground(),"Effect can start after bounded cooldown");sys.Shake.Tick(.06);
        sys.SetLocked(true);Check(!sys.Shake.Active&&Bounds(h)==origin,"Lockdown cancels and restores an active native effect");
        await Frames(30);Check(receiver.Focus(),"Receiver remains available during lockdown");
        var clicks=receiver.Counts();receiver.Move(origin.Position+new Vector2(320,220));receiver.Click();await Frames(6);
        var after=receiver.Counts();Check(after.Down==clicks.Down+1&&after.Up==clicks.Up+1,"Locked overlays do not capture real clicks on the receiver");
        receiver.LockdownChord();await Frames(8);Check(!sys.State.Locked,"Real global hotkey resumes from another process");
        Check(receiver.Focus(),"Receiver refocused for global lockdown");receiver.LockdownChord();await Frames(8);Check(sys.State.Locked,"Real global hotkey locks from another process");
        sys.SetLocked(false);Check(receiver.Focus(),"Receiver refocused for cancellation test");
        for(int i=0;i<210;i++)sys.Shake.Tick(.1);
        Check(sys.Shake.StartForeground(),"User-movement test starts on owned target");sys.Shake.Tick(.06);
        var user=origin.Position+new Vector2I(40,20);SetWindowPos(h,0,user.X,user.Y,0,0,0x215);sys.Shake.Tick(.06);
        Check(!sys.Shake.Active&&Bounds(h).Position==user,"External user movement wins; stale origin is not restored");
        sys.SetStrong(false);
    }
}
