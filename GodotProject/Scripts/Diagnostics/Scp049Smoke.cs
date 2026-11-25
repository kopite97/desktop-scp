using Godot;
using SiteDesktop.App;
using SiteDesktop.Desktop;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Platforms;
using SiteDesktop.Simulation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;
public partial class Scp049Smoke : Node
{
    private readonly List<string> _checks=new();
    private string _output="";
    private Main? _main;
    private readonly List<Window> _owned=new();
    private void Require(bool ok,string name){if(!ok)throw new InvalidOperationException(name);_checks.Add(name);GD.Print("SCP049_CHECK_PASS "+name);}
    private static DesktopScreen Screen=>new(0,new(-1920,1061,1920,1080),new(-1920,1061,1920,1032),96);
    private static PlagueState Create(PlagueSettings? settings=null)=>new(new[]{Screen},new StressState(),settings){AutoSpawn=false};
    private static void Step(PlagueState s,double seconds){for(int i=0;i<(int)Math.Ceiling(seconds/.02);i++)s.Tick(Math.Min(.02,seconds-i*.02));}
    private void Logic()
    {
        var s=Create();int id=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(160,0))!.Value;
        Require(s.Objects[id].Kind==DummyKind.Temporary&&s.Objects.Count==1,"spawn creates only in-memory dummy object");
        Step(s,.4);Require(s.Phase==DoctorPhase.Seeking&&s.TargetId==id&&s.Objects[id].ClaimedBy==PlagueState.DoctorId,"nearest eligible target reserved by doctor");
        Require(!s.TryClaim(id,"other")&&!s.CompleteTreatment(id,"other"),"duplicate owner cannot claim or transform target");
        Step(s,1.7);Require(s.Phase==DoctorPhase.Treating&&s.Transformations==0,"walking reaches target and begins timed treatment");
        Require(!s.CompleteTreatment(id,PlagueState.DoctorId),"completion command rejects early treatment");
        Step(s,3.1);Require(s.Transformations==1&&s.Objects[id].Kind==DummyKind.Zombie&&s.Objects[id].ClaimedBy==null,"treatment atomically changes one target into zombie");
        Require(!s.CompleteTreatment(id,PlagueState.DoctorId)&&s.Transformations==1,"repeated completion cannot duplicate zombie");
        float start=s.Objects[id].Foot.X;Step(s,1);Require(s.Objects[id].Foot.X>start,"zombie walks toward lower corner");
        Step(s,40);Require(s.Objects[id].Foot.X>=Screen.Usable.End.X-252&&s.Objects[id].Foot.X<=Screen.Usable.End.X-40,"zombie patrol stays within corner interval");
        Require(s.Collect(id)&&!s.Collect(id)&&s.Objects.Count==0&&s.Collected==1,"click collection is idempotent and removes object");
        int next=s.Spawn()!.Value;Require(next>id,"object IDs never reuse removed identity");s.Shutdown();
        s=Create();id=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(40,0))!.Value;s.TryClaim(id,"other");Step(s,1);
        Require(s.TargetId==null,"foreign reservation excluded from search");s.Release(id,"other");Step(s,.2);
        Require(s.Phase==DoctorPhase.Treating,"released reservation becomes selectable");s.Collect(id);
        Require(s.TargetId==null&&s.TreatmentAge==0&&s.Phase==DoctorPhase.Idle,"disappearing target cancels treatment immediately");Step(s,4);Require(s.Transformations==0,"removed target never transforms later");
        s=Create(new PlagueSettings{MaxData=2,MaxZombies=1,TreatmentSeconds=.2});int a=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(30,0))!.Value;int b=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(36,0))!.Value;
        Require(s.Spawn()==null,"data count cap rejects extra spawn");Step(s,1);
        Require(s.Transformations==1&&s.Objects.Values.Count(o=>o.Kind==DummyKind.Zombie)==1&&s.Objects.Values.Count(o=>o.Kind!=DummyKind.Zombie)==1,"zombie cap keeps remaining data intact");
        int zombie=s.Objects.Values.Single(o=>o.Kind==DummyKind.Zombie).Id;s.Collect(zombie);Step(s,1);Require(s.Transformations==2,"collecting zombie frees treatment capacity");
        s=Create(new PlagueSettings{DataLifetime=.2});s.SpawnAt(s.Doctor.Body.Foot);Step(s,.3);Require(s.Expired==1&&s.Objects.Count==0&&s.TargetId==null,"data expiry releases any reservation");
        s=Create(new PlagueSettings{TreatmentSeconds=.1,ZombieLifetime=.5});s.SpawnAt(s.Doctor.Body.Foot);Step(s,1.1);Require(s.Transformations==1&&s.Objects.Count==0&&s.Expired==1,"zombie expiry uses reset lifetime after transformation");
        s=Create();id=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(30,0))!.Value;Step(s,.6);
        var position=s.Doctor.Body.Foot;double clock=s.Clock,age=s.Objects[id].Age,treatment=s.TreatmentAge;s.SetVisible(false);Step(s,20);
        Require(s.Clock==clock&&s.Objects[id].Age==age&&s.TreatmentAge==treatment&&s.Doctor.Body.Foot==position,"hide freezes movement, treatment and lifetime clocks");
        Require(s.Spawn()==null&&!s.Collect(id)&&!s.BeginDoctorDrag(position),"hidden state rejects spawn collection and drag");s.SetVisible(true);s.Observe(Array.Empty<WindowObservation>());Step(s,3);
        Require(s.Transformations==1,"restore resumes one treatment without hidden-time catch-up");
        s=Create();s.AutoSpawn=true;s.Tick(100);Require(s.Clock==.1&&s.Objects.Count==0,"long frame is bounded and cannot spawn a backlog");Step(s,6);Require(s.Objects.Count==1,"periodic spawner uses visible simulation time");
        id=s.Objects.Keys.Single();s.BeginObjectDrag(id,s.Objects[id].Foot);s.Drag(s.Objects[id].Foot-new Vector2(0,200));float y=s.Objects[id].Foot.Y;s.SetVisible(false);s.SetVisible(true);
        Require(s.DraggedObject==null&&s.Objects[id].Foot.Y==y,"hide cancels object drag without moving its position");s.Tick(.1);Require(s.Objects[id].Foot.Y>y,"restored dragged object resumes gravity");
        s.BeginDoctorDrag(s.Doctor.Body.Foot);s.Drag(s.Doctor.Body.Foot-new Vector2(0,150));s.EndDrag();s.Tick(.1);Require(s.Doctor.Body.Motion==PlatformMotion.Falling,"doctor right release uses shared gravity");
        s.ReplaceScreens(new[]{new DesktopScreen(1,new(0,0,1280,720),new(0,0,1280,680),144)});Step(s,1);
        Require(s.Doctor.Body.Foot.X>=40&&s.Objects.Values.All(o=>o.Foot.X>=28),"monitor removal recovers doctor and data in physical pixels");
        s.Shutdown();s.SetVisible(true);Step(s,10);Require(s.Stopped&&!s.Visible&&s.Objects.Count==0&&s.TargetId==null,"shutdown is terminal and clears all objects and reservations");
        PlatformLogic();
    }
    private void PlatformLogic()
    {
        var s=Create();var w=new WindowObservation("A",new(-1700,1661,500,240));s.Observe(new[]{w});s.Doctor.Place(new(-1450,1500));int id=s.SpawnAt(new(-1415,1500))!.Value;Step(s,.8);
        Require(s.Phase==DoctorPhase.Treating&&s.Doctor.Body.SupportId=="A"&&s.Objects[id].Physics.Body.SupportId=="A","doctor treats data on same observed window ledge");
        var doctor=s.Doctor.Body.Foot;var data=s.Objects[id].Foot;s.Observe(new[]{w with{Bounds=new(-1660,1641,500,240)}});
        Require(s.Doctor.Body.Foot==doctor+new Vector2(40,-20)&&s.Objects[id].Foot==data+new Vector2(40,-20),"moving ledge carries doctor and data through same physics");
        Require(s.TargetId==null&&s.TreatmentAge==0,"balance interrupts surgery and releases reservation");Step(s,.6);Require(s.Phase==DoctorPhase.Treating,"stable ledge can start fresh treatment");
        s.Observe(Array.Empty<WindowObservation>());Require(s.TargetId==null&&s.Doctor.Body.Motion==PlatformMotion.Falling,"support disappearance cancels treatment immediately");Step(s,1);Require(s.Doctor.Body.SupportId=="floor:0"&&s.Objects[id].Physics.Body.SupportId=="floor:0","both entities fall onto desktop floor");
        s=Create(new PlagueSettings{TargetTimeout=.5});s.Observe(new[]{w});s.SpawnAt(new(-1450,1500));Step(s,.8);Require(s.TargetId==null,"unreachable target releases reservation after bounded search");
        s=Create();var upper=new WindowObservation("B",new(-1100,1561,450,240));s.Observe(new[]{upper,w});s.Doctor.Place(new(-1280,1500));s.SpawnAt(new(-1030,1400));Step(s,8);
        Require(s.Doctor.Body.Jumps>0&&s.Transformations==1,"doctor search uses shared adjacent jump and completes upper ledge treatment");
        var shared=new StressState();s=new(new[]{Screen},shared){AutoSpawn=false};Require(shared.Targets.ContainsKey(PlagueState.DoctorId),"doctor registers with shared stress owner");
        shared.Relieve(s.Doctor.Body.Foot,240,3);Require(s.DoctorStress.Value==37,"SCP-999 relief command reduces actual doctor stress");
        s.BeginDoctorDrag(s.Doctor.Body.Foot);s.Drag(s.Doctor.Body.Foot+new Vector2(100,-100));Require(s.DoctorStress.Position==s.Doctor.Body.Foot,"shared stress position follows doctor drag");s.Shutdown();Require(!shared.Targets.ContainsKey(PlagueState.DoctorId),"shutdown removes doctor from common stress registry");
    }
    private async Task Frames(int n){for(int i=0;i<n;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    public override async void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();for(int i=0;i+1<args.Length;i++)if(args[i]=="--output")_output=args[++i];
            if(_output==""||File.Exists(Path.Combine(_output,"results.json")))throw new InvalidOperationException("New evidence required.");Directory.CreateDirectory(_output);
            Logic();_main=GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();_main.PersistScpSettings=false;_main.EnableSystemOptions=false;_main.EnableStatue=false;_main.UseDesktopIcons=false;AddChild(_main);
            _main.Companion.AutomaticInput=false;var c=_main.Plague!;c.Automatic=false;c.State.AutoSpawn=false;await Frames(5);
            bool native=DisplayServer.GetName()!="headless";
            if(native)await Native(c,args.Contains("--native-input"));
            _main.RequestExit();Require(c.State.Stopped&&c.ObjectWindows.Count==0&&c.DoctorWindow==null&&!_main.Stress.Targets.ContainsKey(PlagueState.DoctorId),"application exit clears plague resources and shared registration");
            File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new{passed=true,checks=_checks.Count,native,nativeInput=args.Contains("--native-input"),tests=_checks,limitations=new[]{"ART-004..007 proposed; code-drawn temporary presentation","Mixed-DPI hardware and long-duration performance not tested"}},new JsonSerializerOptions{WriteIndented=true}));GD.Print("SCP049_SMOKE_PASS checks="+_checks.Count);
        }
        catch(Exception ex){GD.PushError("SCP049_SMOKE_FAILED "+ex);_main?.Plague?.Shutdown();_main?.Companion.Shutdown();GetTree().Quit(1);}
        finally{foreach(var w in _owned)if(IsInstanceValid(w)){w.Hide();w.QueueFree();}}
    }
}
