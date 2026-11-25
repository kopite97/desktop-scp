using Godot;
using SiteDesktop.App;
using SiteDesktop.Desktop;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;
public partial class StatueSmoke : Node
{
    private Main? _main;private string _output="";private readonly List<string> _checks=new();
    private void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);_checks.Add(name);GD.Print("STATUE_CHECK "+name);}
    private static readonly DesktopScreen Lg=new(0,new(-1920,1061,1920,1080),new(-1920,1061,1920,1032),96);
    private static readonly DesktopScreen Other=new(1,new(0,0,1920,1080),new(0,0,1920,1032),96);
    private static readonly DesktopScreen[] Screens={Lg,Other};
    private async Task Frames(int n){for(int i=0;i<n;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private (StatueState,PlagueState,StressState,int) Setup()
    {
        var stress=new StressState();var p=new PlagueState(new[]{Lg},stress){AutoSpawn=false,ChooseAppearance=()=>"fixture"};
        p.BeginDoctorDrag(p.Doctor.Body.Foot);int id=p.SpawnAt(new(-1200,2085))!.Value;for(int i=0;i<5;i++)p.Tick(.1);
        return(new StatueState(new[]{Lg},stress),p,stress,id);
    }
    private void Step(StatueState s,PlagueState p,Vector2? cursor,double time)
    {for(int i=0;i<(int)Math.Ceiling(time/.02);i++)s.Tick(.02,cursor,Screens,p.World);}
    private void Logic()
    {
        var (s,p,stress,id)=Setup();var before=s.Position;var near=s.Position+new Vector2(32,44);
        Step(s,p,near,10);Check(s.Observed&&s.Position==before&&s.Attacks==0,"Observed statue does not move or attack");
        Step(s,p,near+new Vector2(-180,0),9);Check(s.Attacks==0,"Observation radius includes 180px boundary");
        var far=new Vector2(-1900,1200);Step(s,p,far,7.9);Check(s.Attacks==0,"Same-screen unseen delay does not trigger early");
        Step(s,p,far,.2);Check(s.Attacks==1&&p.Objects[id].BrokenAge==0&&p.Objects[id].AppearanceId=="fixture","Unseen threshold breaks same icon object and retains appearance");
        Check(s.Position!=before&&!p.TryClaim(id,PlagueState.DoctorId),"Broken target cannot be reserved for surgery");
        Check(!p.Break(id,StatueState.Id),"Duplicate break cannot apply twice");
        for(int i=0;i<9;i++)p.Tick(.1);Check(!p.Objects.ContainsKey(id),"Broken icon removed after bounded visual lifetime");s.Shutdown();p.Shutdown();
        (s,p,stress,id)=Setup();p.TryClaim(id,PlagueState.DoctorId);Step(s,p,new(100,200),.1);
        Check(s.Attacks==0&&p.Objects[id].ClaimedBy==PlagueState.DoctorId,"Other-monitor attack does not steal doctor reservation");
        p.Release(id,PlagueState.DoctorId);Step(s,p,new(100,200),2.1);Check(s.Attacks==1,"Other monitor triggers after retry cooldown without eight-second wait");s.Shutdown();p.Shutdown();
        (s,p,stress,id)=Setup();Step(s,p,far,7);Step(s,p,near,.1);Step(s,p,far,1.2);Check(s.Attacks==0,"Observation resets unseen accumulation");
        s.SetVisible(false);Step(s,p,new(100,200),20);Check(s.Attacks==0,"Hidden statue cannot attack");s.SetVisible(true);
        Step(s,p,null,10);Step(s,p,new(9000,9000),10);Check(s.Attacks==0&&s.Observed,"Missing or out-of-desktop pointer fails closed");
        s.Stress.Set(72);stress.Relieve(s.Physics.Body.Foot,200,3);Check(s.Stress.Value==69,"SCP-999 common relief lowers actual statue stress");
        Step(s,p,far,4.1);Check(s.Attacks==0,"Relief below stress threshold restores long delay");
        s.Stress.Set(80);Step(s,p,far,.1);Check(s.Attacks==1,"High stress uses four-second unseen delay");
        s.Shutdown();p.Shutdown();Check(stress.Targets.Count==0,"Shutdown releases common stress registrations");
        (s,p,stress,id)=Setup();p.Collect(id);Step(s,p,new(100,200),5);Check(s.Attacks==0,"Empty or removed target is a safe no-op");
        s.BeginDrag(s.Physics.Body.Foot);Step(s,p,new(100,200),10);Check(s.Attacks==0&&s.Dragging,"User drag suppresses autonomous attack");s.EndDrag();s.Shutdown();p.Shutdown();
    }
    public override async void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();for(int i=0;i+1<args.Length;i++)if(args[i]=="--output")_output=args[++i];
            if(_output==""||File.Exists(Path.Combine(_output,"results.json")))throw new InvalidOperationException("New evidence required");Directory.CreateDirectory(_output);
            Logic();bool native=DisplayServer.GetName()!="headless";
            if(native)await Native();
            File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new{passed=true,native,checks=_checks},new JsonSerializerOptions{WriteIndented=true}));
            GD.Print("STATUE_PASS");if(_main!=null)_main.RequestExit();else GetTree().Quit();
        }
        catch(Exception ex){GD.PushError("STATUE_FAILED "+ex);_main?.RequestExit();GetTree().Quit(1);}
    }
    private async Task Native()
    {
        _main=GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();_main.PersistScpSettings=false;_main.EnableSystemOptions=false;AddChild(_main);_main.Companion.AutomaticInput=false;
        var c=_main.Plague!;var statue=_main.Statue!;c.Automatic=false;statue.Automatic=false;c.State.AutoSpawn=false;
        for(int i=0;i<900&&_main.Icons!.CatalogCount==0;i++)await Frames(1);
        Check(_main.Icons!.CatalogCount>0,"Native product has real runtime icons");
        foreach(int old in c.State.Objects.Keys.ToArray())c.State.Collect(old);
        c.State.BeginDoctorDrag(c.State.Doctor.Body.Foot);var area=c.State.Screens[0].Usable;
        int id=c.State.SpawnAt(area.Position+new Vector2(800,area.Size.Y-8))!.Value;
        for(int i=0;i<5;i++)c.Advance(.1);string appearance=c.State.Objects[id].AppearanceId!;
        var screens=OverlayController.ReadScreens();Vector2 far=area.Position+new Vector2(10,10);
        for(int i=0;i<410;i++)statue.State.Tick(.02,far,screens,c.Objects.State);
        statue.Publish();c.Publish();await Frames(15);
        Check(statue.State.Attacks==1&&c.State.Objects[id].BrokenAge!=null,"Native product state breaks runtime icon");
        Check(c.State.Objects[id].AppearanceId==appearance&&c.ObjectWindows[id].Icon!=null,"Native fragments use original icon texture");
        Check(WindowsDesktop.GetBounds(statue.Actor!).Position==(Vector2I)statue.State.Position.Round(),"Statue native position matches LG physical coordinates");
        using(var image=c.ObjectWindows[id].GetTexture().GetImage())
        {
            var source=c.ObjectWindows[id].Icon!;int mismatch=0;
            for(int y=0;y<20;y++)for(int x=0;x<source.Pixels.Width;x++)
            {
                int sy=y,offset=(sy*source.Pixels.Width+x)*4;var col=image.GetPixel((int)source.Offset.X-4+x,(int)source.Offset.Y-2+y);
                if(Math.Abs(col.A-source.Pixels.Rgba[offset+3]/255f)>.02)mismatch++;
            }
            Check(mismatch==0,"Rendered broken top fragment retains source alpha");
        }
        for(int i=0;i<9;i++)c.Advance(.1);await Frames(3);Check(!c.State.Objects.ContainsKey(id)&&!c.ObjectWindows.ContainsKey(id),"Broken icon native window cleaned up");
        float stress=statue.State.Stress.Value;_main.Stress.Relieve(statue.State.Physics.Body.Foot,10,3);Check(statue.State.Stress.Value==stress-3,"Actual product statue shares relief owner with SCP-999");
        statue.SetVisible(false);await Frames(3);Check(!statue.Actor!.Visible,"Hide removes statue native window");statue.SetVisible(true);await Frames(3);Check(statue.Actor.Visible,"Restore returns statue native window");
        statue.Automatic=true;await Frames(10);
        if(WindowsDesktop.TryGetPointer(out var cursor,out _))
        {
            bool expected=((Vector2)cursor).DistanceTo(statue.State.Position+new Vector2(32,44))<=180;
            Check(statue.State.Observed==expected,"Real Windows cursor observation reaches product statue state");
        }
        statue.Shutdown();Check(statue.Actor==null&&!_main.Stress.Targets.ContainsKey(StatueState.Id),"Statue shutdown removes window and stress target");
    }
}
