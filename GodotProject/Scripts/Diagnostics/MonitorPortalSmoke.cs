using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Behavior;
using SiteDesktop.App;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
using System.Threading.Tasks;
using SiteDesktop.Platforms;
using SiteDesktop.Simulation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SiteDesktop.Diagnostics;
public partial class MonitorPortalSmoke : Node
{
    private readonly List<string> _checks=new();
    private string _output="";
    private static DesktopScreen[] Screens=>new[]{new DesktopScreen(0,new(-1280,0,1280,800),new(-1280,0,1280,760),96),new DesktopScreen(1,new(0,0,1600,900),new(0,0,1600,860),144)};
    private void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);_checks.Add(name);GD.Print("PORTAL_CHECK "+name);}
    private Main? _main;
    public override async void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();_output=args[Array.IndexOf(args,"--output")+1];Directory.CreateDirectory(_output);
            if(File.Exists(Path.Combine(_output,"results.json")))throw new InvalidOperationException("Fresh evidence directory required");
            if(args.Contains("--baseline")){Baseline();}else Verify();
            if(!args.Contains("--baseline")&&DisplayServer.GetName()!="headless")await Native(args.Contains("--native-input"));
            Save();_main?.RequestExit();GetTree().Quit();
        }
        catch(Exception ex){GD.PushError(ex.ToString());_main?.RequestExit();GetTree().Quit(1);}
    }
    private async Task Frames(int n){for(int i=0;i<n;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private async Task Native(bool input)
    {
        await Frames(2);
        _main=GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();_main.PersistScpSettings=false;_main.UseDesktopIcons=false;AddChild(_main);
        _main.Systems!.AutomaticMetrics=false;_main.Systems.PlayAlarm=false;_main.Scps.SetEnabled("scp-999",false);_main.Scps.SetEnabled("scp-173",false);
        var c=_main.Plague!;var d=c.State;_main.Objects!.Automatic=false;d.World.AutoSpawn=false;
        var screens=_main.Environment.Current.Screens.ToArray();
        File.WriteAllText(Path.Combine(_output,"screens.json"),JsonSerializer.Serialize(screens.Select(x=>new{index=x.Index,x=x.Bounds.Position.X,y=x.Bounds.Position.Y,width=x.Bounds.Size.X,height=x.Bounds.Size.Y,workX=x.Usable.Position.X,workY=x.Usable.Position.Y,workWidth=x.Usable.Size.X,workHeight=x.Usable.Size.Y,dpi=x.Dpi}),new JsonSerializerOptions{WriteIndented=true}));
        Check(screens.Length>=2,"native environment exposes at least two monitors");
        d.ObserveDoctor(Array.Empty<WindowObservation>());d.World.Observe(Array.Empty<WindowObservation>());
        void Publish(){_main.Objects.Publish();c.PublishBody();}
        void Step(double time){Advance(d,time);Publish();}
        void Prepare(int source,int dest)
        {
            d.Behavior.Reset();foreach(int id in d.Objects.Keys.ToArray())d.World.Collect(id);
            var from=screens[source];var to=screens[dest];float direction=Math.Sign(to.Bounds.GetCenter().X-from.Bounds.GetCenter().X);
            float x=direction<0?from.Usable.Position.X+88:direction>0?from.Usable.End.X-88:from.Usable.GetCenter().X;
            Place(d,new(x,from.Usable.End.Y-8));d.World.SpawnAt(new(to.Usable.GetCenter().X,to.Usable.End.Y-8));
            for(int i=0;i<100&&!d.Behavior.Portal.Active;i++)Step(.02);
            Check(d.Behavior.Portal.Active,"native transfer starts "+source+" to "+dest);
        }
        using var receiver=input?new NativeInputFixture(_output):null;
        if(receiver!=null)
        {
            for(int i=0;i<180&&!receiver.Ready;i++)await Frames(1);Check(receiver.Ready,"owned input receiver ready");
            var all=screens.Select(s=>s.Usable).Aggregate((a,b)=>a.Merge(b));receiver.Cover(all);await Frames(5);
            bool focused=false;for(int i=0;i<180&&!focused;i++){focused=receiver.Focus();await Frames(1);}Check(focused,"owned receiver acquires foreground before input");
        }
        for(int direction=0;direction<2;direction++)
        {
            Prepare(direction,1-direction);Step(.2);await Frames(5);
            var frame=d.Behavior.Portal.Frame!;var portal=c.Portals!.Departure!;
            Check(portal.Visible&&c.DoctorWindow!.Visible,"departure portal and fading body displayed");
            var bounds=WindowsDesktop.GetBounds(portal);Check(screens[direction].Usable.Encloses(bounds),"departure effect stays in source work area");
            if(receiver!=null)
            {
                receiver.PlaceBehind(portal);receiver.Move(frame.Departure-new Vector2(0,60));Publish();await Frames(8);
                using(var image=DisplayServer.ScreenGetImageRect(new(WindowsDesktop.ToGodotPosition(bounds.Position),bounds.Size)))
                {
                    int bright=0;for(int y=0;y<image.GetHeight();y++)for(int x=0;x<image.GetWidth();x++){var color=image.GetPixel(x,y);if(color.G>.5f&&color.B>.5f&&color.R<.8f)bright++;}
                    image.SavePng(Path.Combine(_output,$"portal-desktop-{direction}.png"));Check(bright>100,"portal is visible in actual desktop capture "+direction+" pixels="+bright);
                }
                var counts=receiver.Counts();receiver.Click();await Frames(5);
                Check(receiver.Counts().Down==counts.Down+1&&receiver.Counts().Up==counts.Up+1,"visible portal and transfer body pass actual click");
            }
            Step(.3);await Frames(3);Check(!c.DoctorWindow!.Visible&&d.Behavior.Portal.Committed,"body hidden while committed between monitors");
            _main.Systems.SetLocked(true);double age=d.Behavior.Portal.Age;Step(.5);await Frames(3);
            Check(d.Behavior.Portal.Age==age&&!portal.Visible&&!c.DoctorWindow.Visible,"native lockdown freezes portal and hides windows");
            _main.Systems.SetLocked(false);Step(.2);await Frames(3);Check(c.DoctorWindow.Visible&&c.Portals.Arrival!.Visible,"native resume restores arrival body and portal");
            if(receiver!=null)Check(receiver.IsForeground,"portal publication does not take foreground");
            Step(.5);await Frames(3);Check(c.Portals.WindowCount==0&&!d.Behavior.Portal.Active,"completed transfer releases effect windows");
            Check(screens[1-direction].Bounds.HasPoint((Vector2I)d.Doctor.Body.Foot),"native actor is on destination monitor");
        }
        for(int i=0;i<20;i++){Prepare(i%2,1-i%2);Step(1.1);await Frames(2);Check(c.Portals!.WindowCount==0,"effect lifetime returns to zero cycle "+i);}
        if(receiver!=null)
        {
            d.Behavior.Reset();foreach(int id in d.Objects.Keys.ToArray())d.World.Collect(id);
            var a=screens[0].Usable;var b=screens[1].Usable;Place(d,new(a.GetCenter().X,a.End.Y-8));Publish();await Frames(3);
            Vector2 point=d.Doctor.Body.Foot-new Vector2(0,50);receiver.PlaceBehind(c.DoctorWindow!);receiver.Move(point);Publish();await Frames(4);receiver.Press(true);await Frames(4);
            Check(d.Phase==DoctorPhase.Dragging,"native right press enters doctor drag");
            c.Automatic=true;receiver.Move(new(b.GetCenter().X,b.End.Y-58));await Frames(10);receiver.Release();await Frames(25);c.Automatic=false;
            Check(screens[1].Bounds.HasPoint((Vector2I)d.Doctor.Body.Foot)&&d.Behavior.RemoteDelay>0,"native cross-monitor drag ends with remote grace");
            int? spawned=d.World.Spawn();Check(spawned!=null&&screens[1].Bounds.HasPoint((Vector2I)d.Objects[spawned.Value].Foot),"post-drag spawn follows doctor on destination");
        }
        // Dispose the receiver before the caller quits the tree; no awaits after Quit.
    }
    private void Baseline()
    {
            var world=new DesktopObjectState(Screens);world.Spawn();world.Spawn();
            Check(world.Objects.Values.Any(o=>o.Foot.X<0)&&world.Objects.Values.Any(o=>o.Foot.X>0),"baseline: automatic spawn distributes across both monitors");
            var doctor=new PlagueState(Screens,new(),world:world);world.AutoSpawn=false;
            for(int i=0;i<100;i++){world.Tick(.02);doctor.TickDoctor(.02);}
            Check(!doctor.Behavior.Movement.CanReach(new(new(200,852),"floor:1")),"baseline: other monitor floor is unreachable");
    }
    private void Save()
    {
        File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new{passed=true,checks=_checks.Count,tests=_checks},new JsonSerializerOptions{WriteIndented=true}));GD.Print("PORTAL_PASS");
    }
    private static void Advance(PlagueState doctor,double seconds)
    {for(int i=0;i<(int)Math.Ceiling(seconds/.02);i++){doctor.World.Tick(.02);doctor.TickDoctor(.02);}}
    private static PlagueState Doctor(DesktopScreen[]? screens=null)
    {var d=new PlagueState(screens??Screens,new());d.World.AutoSpawn=false;return d;}
    private static void Place(PlagueState d,Vector2 foot){d.Doctor.Place(foot);for(int i=0;i<15;i++)d.Doctor.Tick(.02);}
    private void Verify()
    {
        var d=Doctor();d.World.SpawnAnchorProvider=()=>new(ScreenLocator.Footprint(d.Doctor.Body.Foot,40,112),d.Phase==DoctorPhase.Dragging||d.Behavior.Portal.Active);
        for(int i=0;i<6;i++)d.World.Spawn();
        Check(d.Objects.Count==6&&d.Objects.Values.All(o=>o.Foot.X<0),"automatic generation stays on anchor monitor and respects global cap");
        var original=d.Objects.Values.First();double age=original.Age;var foot=original.Foot;
        d.BeginDoctorDrag(d.Doctor.Body.Foot);d.Drag(new(500,852));
        Check(d.World.Spawn()==null&&original.Foot==foot&&original.Age==age,"drag suspends spawning and leaves old icons untouched");d.EndDrag();
        d.World.Collect(original.Id);int spawned=d.World.Spawn()!.Value;Check(d.Objects[spawned].Foot.X>0&&d.Objects.Count==6,"new monitor receives clones without multiplying cap");
        var policy=new ObjectSpawnPolicy();Check(policy.Select(Screens,new(new Rect2(-100,100,80,100)))?.Index==0,"anchor resolves negative origin");
        Check(policy.Select(Screens,new(new Rect2(-20,100,40,100)))?.Index==0,"equal overlap retains previous screen");
        Check(policy.Select(Screens,null)==null&&policy.Select(Screens,new(new Rect2(99999,99999,40,40)))==null,"missing anchor and unknown region cannot create clones");
        var app=GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();app.PersistScpSettings=false;app.UseDesktopIcons=false;app.EnableSystemOptions=false;AddChild(app);
        Check(app.Objects!.State.Objects.Count==1,"initial spawn occurs after actor registration");
        app.Scps.SetEnabled("scp-049",false);app.Objects.SpawnData();Check(app.Objects.State.Objects.Count==2,"173-only consumer keeps spawning");
        app.Scps.SetEnabled("scp-173",false);Check(app.Objects.State.Spawn()==null,"no consumer means no clones");app.QueueFree();
        d=Doctor();int id=d.World.SpawnAt(new(400,852))!.Value;
        Advance(d,35);
        Check(d.Transformations==1&&d.Doctor.Body.Foot.X>0&&d.Behavior.Portal.Commits==1,"cross-monitor route transfers once then treats target");
        Check(d.Objects[id].ClaimedBy==null&&!d.Behavior.Portal.Active,"arrival releases reservation after treatment");
        d=Doctor();Place(d,new(200,852));d.World.SpawnAt(new(-400,752));Advance(d,30);
        Check(d.Transformations==1&&d.Doctor.Body.Foot.X<0,"right to left portal treats target");
        d=Doctor();d.World.SpawnAt(new(-600,752));Place(d,new(300,852));d.BeginDoctorDrag(d.Doctor.Body.Foot);d.Drag(new(-900,752));d.EndDrag();Advance(d,.4);
        Check(d.Behavior.RemoteDelay>4.5&&!d.Behavior.Portal.Active,"cross-monitor drag starts grace after landing");
        d.World.SpawnAt(new(100,852));Advance(d,1);
        Check(d.TargetId!=null&&d.Objects[d.TargetId.Value].Foot.X<0,"local target beats remote target during grace");
        foreach(double stage in new[]{.2,.5,.8})
        {
            d=Doctor();Place(d,new(-88,752));id=d.World.SpawnAt(new(300,852))!.Value;
            for(int i=0;i<100&&!d.Behavior.Portal.Active;i++)Advance(d,.02);
            Check(d.Behavior.Portal.Active,"portal starts at departure gate");Advance(d,stage);
            double portalAge=d.Behavior.Portal.Age;var position=d.Doctor.Body.Foot;int commits=d.Behavior.Portal.Commits;
            Check(!d.BeginDoctorDrag(position),"transfer refuses new body drag");
            d.SetDoctorActive(false,false);Advance(d,2);Check(d.Behavior.Portal.Age==portalAge&&d.Doctor.Body.Foot==position,"lockdown freezes transfer stage "+stage);
            d.SetDoctorActive(true,false);d.World.Collect(id);Advance(d,.2);
            Check(!d.Behavior.Portal.Active&&d.TargetId==null&&d.Behavior.Portal.Commits==commits,"target removal cancels without a late commit "+stage);
            Check((stage<.4)==(d.Doctor.Body.Foot.X<0),"cancel retains committed side "+stage);
        }
        d=Doctor();Place(d,new(-88,752));d.World.SpawnAt(new(300,852));Advance(d,.6);d.SetDoctorActive(false,true);Advance(d,2);
        Check(d.TargetId==null&&!d.Behavior.Portal.Active&&d.Objects.Values.All(o=>o.ClaimedBy==null),"individual OFF cancels portal and claim");
        d=Doctor();Place(d,new(-88,752));d.World.SpawnAt(new(300,852));Advance(d,.6);d.ReplaceDoctorScreens(new[]{Screens[0]});Advance(d,.3);
        Check(!d.Behavior.Portal.Active&&d.Doctor.Body.Foot.X<0,"removed destination recovers on remaining screen");
        d=Doctor();d.ObserveDoctor(new[]{new WindowObservation("high",new(100,100,400,150))});d.World.Observe(d.World.Observations.Append(new WindowObservation("high",new(100,100,400,150))).ToArray());d.World.SpawnAt(new(200,100));Advance(d,10);
        Check(d.TargetId==null&&d.Behavior.Portal.Commits==0,"portal does not bypass unreachable destination ledge");
        var wide=new[]{new DesktopScreen(0,new(0,0,3840,1200),new(0,0,3840,1160),144)};d=Doctor(wide);Place(d,new(60,1152));d.World.SpawnAt(new(2800,1152));Advance(d,45);
        Check(d.Transformations==1&&d.Behavior.Portal.Commits==0,"long local walk survives original twelve-second timeout");
        var vertical=new[]{Screens[0],new DesktopScreen(2,new(-1280,-1200,1280,900),new(-1280,-1200,1280,850),192)};d=Doctor(vertical);d.World.SpawnAt(new(-900,-358));Advance(d,30);
        Check(d.Transformations==1&&d.Behavior.Portal.Commits==1,"separated vertically stacked screens transfer in physical coordinates");
        d=Doctor();Place(d,new(-300,752));d.World.SpawnAt(new(-500,752));d.BeginDoctorDrag(d.Doctor.Body.Foot);d.Drag(new(88,852));d.EndDrag();Advance(d,4.5);
        Check(d.TargetId==null&&d.Behavior.Portal.Commits==0,"remote target is not selected during drag grace");Advance(d,2);
        Check(d.TargetId!=null&&d.Behavior.Portal.Commits==1,"remote target becomes available after grace");
        d=Doctor();Place(d,new(-88,752));int remote=d.World.SpawnAt(new(88,852))!.Value;int local=d.World.SpawnAt(new(-1100,752))!.Value;Advance(d,.4);
        Check(d.TargetId==local,"far local target is preferred over a closer remote target");
        var three=Screens.Append(new DesktopScreen(2,new(2000,-1000,800,600),new(2000,-1000,800,560),192)).ToArray();var w=new DesktopObjectState(three);w.SpawnAnchorProvider=()=>new(new Rect2(2300,-700,80,112));w.Spawn();
        Check(w.Objects.Values.Single().Foot.X>2000,"third separated monitor receives anchor-local spawn");
        w.SpawnAnchorProvider=()=>new(new Rect2(2300,-700,80,112),true);Check(w.Spawn()==null,"transit suspension skips spawn without fallback");
        var old=w.Objects.Values.Single();w.AutoSpawn=false;for(int i=0;i<4600;i++)w.Tick(.02);Check(w.Objects.Count==0&&w.Expired==1,"remote leftovers retain natural ninety-second lifetime");
        d=Doctor(new[]{Screens[0]});Check(d.Behavior.Routes.Plan(new(new(100,852),"floor:1"))==null,"selected single-monitor scope rejects external destination");
        d=Doctor();Place(d,new(-88,752));d.World.SpawnAt(new(300,852));Advance(d,.6);d.Shutdown();Advance(d,1);Check(!d.Behavior.Portal.Active&&d.Stopped,"shutdown cancels mid-transfer without resurrection");
        var body=new PlatformWorld(Screens);body.Place(new(-88,752));for(int i=0;i<30;i++)body.Tick(.02);var nav=new PlatformNavigator(body);var planner=new MonitorRoutePlanner(body,nav);var route=planner.Plan(new(new(400,852),"floor:1"))!;
        int completed=0;var portal=new PortalTransferState(body,_=>true,_=>completed++);portal.Prepare(route);portal.Enter();portal.Tick(.5);portal.Exit();portal.Tick(1);
        Check(portal.Commits==1&&completed==0&&!portal.Active,"generic actor can cancel transfer without stale completion");
        portal.Prepare(route);portal.Enter();portal.Tick(1.1);portal.Tick(1.1);Check(completed==1,"generic transfer completes exactly once even before consumer exits");portal.Exit();
        File.WriteAllText(Path.Combine(_output,"transitions.jsonl"),string.Join("\n",d.Behavior.Machine.Transitions.Select(t=>JsonSerializer.Serialize(t))));
    }
}
