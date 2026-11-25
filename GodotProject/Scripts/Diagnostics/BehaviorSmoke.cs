using Godot;
using SiteDesktop.Behavior;
using SiteDesktop.App;
using SiteDesktop.Companions;
using SiteDesktop.Desktop;
using SiteDesktop.Overlay;
using SiteDesktop.Platforms;
using SiteDesktop.Simulation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;
public partial class BehaviorSmoke : Node
{
    private readonly List<string> _checks=new();
    private string _output="";
    private Main? _main;
    private void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);_checks.Add(name);GD.Print("BEHAVIOR_CHECK "+name);}
    private static DesktopScreen[] Screens=>new[]{new DesktopScreen(0,new(0,0,1280,800),new(0,0,1280,760),96)};
    private static Scp999State Companion()=>new(Screens,new(Scp999Window.AssetPath+"Scp999.contract.json"),new("res://Assets/Sprites/vfx/Scp999Reaction/Scp999Reaction.contract.json"),new("res://Assets/Sprites/vfx/Scp999Trail/Scp999Trail.contract.json"),new());
    private sealed class ReplayEnvironment : IDesktopEnvironmentSource
    {
        public int Reads { get; private set; }
        public DesktopEnvironmentSnapshot Current { get; }=new(7,1,Screens,Screens,Array.Empty<WindowObservation>(),new Vector2(50,50),false,false);
        public DesktopEnvironmentSnapshot Capture(double delta,bool force=false){Reads++;return Current;}
    }
    private void Baseline()
    {
        var s=new PlagueState(Screens,new()){AutoSpawn=false};
        s.Doctor.Place(new(40,752));for(int i=0;i<30;i++)s.Doctor.Tick(.02);
        var start=s.Doctor.Body.Foot;s.Doctor.Walk(-1);for(int i=0;i<50;i++){s.Doctor.Tick(.02);s.Animation.Update(s,.02);}
        Check(s.Doctor.Body.Foot==start&&s.Doctor.Body.Velocity.X<0&&s.Animation.Tag=="move","baseline: clamped doctor still reports walking");
        var c=Companion();var foot=c.Foot;for(int i=0;i<1500;i++)c.Tick(.02,new(1200,0));
        Check(c.Foot==foot,"baseline: distant cursor leaves companion stationary for 30 seconds");
        var w=new WindowObservation("unreachable",new(10,200,400,200));s=new(Screens,new()){AutoSpawn=false};s.Observe(new[]{w});s.SpawnAt(new(100,200));
        int claims=0;int? last=null;for(int i=0;i<1800;i++){s.Tick(.02);if(last==null&&s.TargetId!=null)claims++;last=s.TargetId;}
        Check(claims>=2&&s.Transformations==0,"baseline: unreachable target repeatedly reserved");
        File.WriteAllText(Path.Combine(_output,"baseline.json"),JsonSerializer.Serialize(new{clampedVelocity=-80,stationarySeconds=30,claims,transformations=s.Transformations}));
    }
    private async Task Measure()
    {
        _main=GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();_main.PersistScpSettings=false;_main.UseDesktopIcons=false;AddChild(_main);
        var samples=new List<object>();var process=Process.GetCurrentProcess();
        for(int run=0;run<3;run++)
        {
            await ToSignal(GetTree().CreateTimer(5),SceneTreeTimer.SignalName.Timeout);
            long observations=_main.Environment.ObservationCount;long bytes=GC.GetTotalAllocatedBytes();var cpu=process.TotalProcessorTime;var watch=Stopwatch.StartNew();var frames=new List<double>();
            while(watch.Elapsed.TotalSeconds<20){var t=Stopwatch.GetTimestamp();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);frames.Add(Stopwatch.GetElapsedTime(t).TotalMilliseconds);}
            Check(_main.Environment.ObservationCount-observations<=401,"shared OS observation bounded to 20Hz run "+run);
            samples.Add(new{run,windowObservations=_main.Environment.ObservationCount-observations,seconds=watch.Elapsed.TotalSeconds,cpuMs=(process.TotalProcessorTime-cpu).TotalMilliseconds,allocatedBytes=GC.GetTotalAllocatedBytes()-bytes,frames=frames.Count,meanFrameMs=frames.Average(),p95FrameMs=frames.OrderBy(x=>x).ElementAt((int)(frames.Count*.95))});
            GD.Print("BEHAVIOR_MEASURE "+run);
        }
        File.WriteAllText(Path.Combine(_output,"metrics.json"),JsonSerializer.Serialize(new{samples,conditions="3 SCPs; icon worker disabled; 5s warmup +20s each; real desktop input; not fixed-input CPU equivalence",limitations="frame wall interval includes FPS pacing; OS changes and autonomous behavior affect CPU"},new JsonSerializerOptions{WriteIndented=true}));
        long reads=_main.Environment.ObservationCount;for(int i=0;i<20;i++)_main.Environment.Capture(0,true);Check(_main.Environment.ObservationCount-reads<=1,"multiple consumers share same-frame observation");
        _main.RequestExit();
    }
    public override async void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();int p=Array.IndexOf(args,"--output");_output=args[p+1];Directory.CreateDirectory(_output);
            if(File.Exists(Path.Combine(_output,"results.json")))throw new InvalidOperationException("Fresh evidence directory required");
            if(args.Contains("--baseline"))Baseline();else Verify();
            if(args.Contains("--measure"))await Measure();
            File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new{passed=true,checks=_checks.Count,tests=_checks},new JsonSerializerOptions{WriteIndented=true}));
            GD.Print("BEHAVIOR_PASS checks="+_checks.Count);GetTree().Quit();
        }
        catch(Exception ex){GD.PushError("BEHAVIOR_FAILED "+ex);_main?.RequestExit();GetTree().Quit(1);}
    }
    private void Verify()
    {
        var body=new PlatformWorld(Screens,new(){HalfWidth=40});body.Place(new(40,752));for(int i=0;i<20;i++)body.Tick(.02);
        body.Walk(-1);body.Tick(.1);Check(body.Body.Velocity.X==0&&body.WalkDistance==0,"clamped floor reports no actual walk");
        long revision=body.Revision;body.Observe(Array.Empty<WindowObservation>());Check(revision==body.Revision,"unchanged geometry retains revision");
        var move=new MovementController(body);var goal=new MovementGoal(new(300,752),"floor:0");
        for(int i=0;i<200;i++){move.Update(goal,.02);body.Tick(.02);}
        Check(move.Status==MovementStatus.Arrived&&Math.Abs(body.Body.Foot.X-300)<9,"shared movement arrives on floor");
        body.Observe(new[]{new WindowObservation("high",new(0,200,400,200))});
        Check(!move.CanReach(new(new(100,200),"high")),"unreachable high ledge rejected");
        int searches=move.Navigator.Searches;
        for(int i=0;i<100;i++)move.CanReach(new(new(100+i,200),"high"));
        Check(move.Navigator.Searches==searches,"unchanged support pair reuses failed route despite target motion");
        body.Observe(new[]{new WindowObservation("step",new(200,620,400,100))});move.Reset();
        var upper=new MovementGoal(new(330,620),"step");for(int i=0;i<300;i++){move.Update(upper,.02);body.Tick(.02);}
        Check(move.Status==MovementStatus.Arrived&&body.Body.Jumps>0,"shared route jumps to reachable ledge");
        body.Observe(new[]{new WindowObservation("step",new(220,620,400,100))});
        Check(body.CarryDelta.X==20,"platform carry reported separately");
        int enters=0,exits=0;var machine=new StateMachine();var second=new StateMachine();
        var idle=new ActionState("one",_=>{},()=>enters++,()=>exits++);var other=new ActionState("two",_=>{});
        machine.Request(idle,"first");machine.Tick(.1);machine.Request(idle,"same");machine.Tick(.1);
        second.Request(other,"separate");second.Tick(.7);
        Check(enters==1&&exits==0&&Math.Abs(machine.Age-.2)<1e-8&&Math.Abs(second.Age-.7)<1e-8,"machines have independent state and time; same-state request is idempotent");
        long generation=machine.Generation;machine.Cancel();Check(exits==1&&machine.Current==null&&generation!=machine.Generation,"cancel exits once and invalidates generation");
        var source=new ReplayEnvironment();var runtime=new ScpRuntime{Environment=source};var order=new List<int>();
        DesktopEnvironmentSnapshot? first=null;bool same=true;
        runtime.Register((dt,s)=>{first=s;order.Add(1);});runtime.Register((dt,s)=>{same&=ReferenceEquals(first,s);order.Add(2);});
        for(int i=0;i<100;i++)runtime.Advance(.02);
        Check(source.Reads==100&&runtime.Updates==100&&same&&order.SequenceEqual(Enumerable.Range(0,200).Select(i=>i%2+1)),"replayed environment is captured once and shared in registration order");
        runtime.Shutdown();runtime.Advance(.02);Check(source.Reads==100&&order.Count==200,"shutdown prevents further observation and actor ticks");runtime.Free();
        var companion=Companion();float walked=0;for(int i=0;i<1500;i++){companion.Tick(.02,new(1200,0));walked+=companion.Physics.WalkDistance;}
        Check(walked>150,"distant cursor permits autonomous wandering");
        Check(companion.Foot.X>=64&&companion.Foot.X<=1216,"wander remains within actor footprint bounds");
        var before=companion.Foot;double behaviorAge=companion.Behavior.Machine.Age;companion.SetVisible(false);for(int i=0;i<200;i++)companion.Tick(.02,new(1200,0));
        Check(companion.Foot==before&&companion.Behavior.Machine.Age==behaviorAge,"lockdown freezes companion behavior time and movement");companion.SetVisible(true);
        for(int i=0;i<50;i++)companion.Tick(.02,companion.Foot);
        long sequence=companion.Behavior.Machine.Generation;for(int i=0;i<100;i++)companion.Tick(.02,companion.Foot);
        Check(companion.Behavior.Machine.Generation==sequence,"cursor dead zone does not churn FSM transitions");
        var doctor=new PlagueState(Screens,new()){AutoSpawn=false};
        var high=new WindowObservation("high",new(20,200,400,150));doctor.Observe(new[]{high});int target=doctor.SpawnAt(new(100,200))!.Value;
        for(int i=0;i<1600;i++)doctor.Tick(.02);
        Check(doctor.TargetId==null&&doctor.Objects[target].ClaimedBy==null&&doctor.Behavior.Movement.Navigator.Searches<=3,"unreachable target has bounded attempts and releases reservation");
        doctor.SpawnAt(doctor.Doctor.Body.Foot+new Vector2(100,0));for(int i=0;i<400;i++)doctor.Tick(.02);
        Check(doctor.Transformations==1,"reachable alternative is treated despite unreachable closer target");
        doctor.Observe(Array.Empty<WindowObservation>());for(int i=0;i<800;i++)doctor.Tick(.02);
        Check(doctor.Transformations==2,"environment change allows failed target to become reachable");
        doctor=new PlagueState(Screens,new()){AutoSpawn=false};doctor.SpawnAt(doctor.Doctor.Body.Foot+new Vector2(30,0));for(int i=0;i<50;i++)doctor.Tick(.02);
        double treatment=doctor.TreatmentAge;doctor.SetVisible(false);for(int i=0;i<100;i++)doctor.Tick(.02);
        Check(doctor.TreatmentAge==treatment&&doctor.TargetId!=null,"lockdown preserves treatment and reservation without advancing time");
        doctor.SetDoctorActive(false,true);Check(doctor.TargetId==null&&doctor.TreatmentAge==0,"individual OFF during lockdown cancels preserved treatment");
        File.WriteAllText(Path.Combine(_output,"transitions.jsonl"),string.Join("\n",companion.Behavior.Machine.Transitions.Select(t=>JsonSerializer.Serialize(t))));

    }
}
