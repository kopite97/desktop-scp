using Godot;
using SiteDesktop.App;
using SiteDesktop.App.Scps;
using SiteDesktop.Desktop;
using SiteDesktop.Simulation;
using SiteDesktop.Platform.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;

public partial class ScpActivationSmoke : Node
{
    private Main? _main;
    private string _output="";
    private readonly List<string> _checks=new();
    private static readonly string[] Ids={"scp-999","scp-049","scp-173"};
    private void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);_checks.Add(name);GD.Print("ACTIVATION_CHECK "+name);}
    private async Task Frames(int n){for(int i=0;i<n;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private Main Create(string? settings=null,bool doctor=true)
    {
        var app=GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();
        app.PersistScpSettings=settings!=null;app.SettingsPath=settings;app.UseDesktopIcons=false;app.EnablePlague=doctor;AddChild(app);
        app.Systems!.AutomaticMetrics=false;app.Systems.PlayAlarm=false;app.Companion.AutomaticInput=false;app.Objects!.Automatic=false;app.Objects.State.AutoSpawn=false;app.Statue!.Automatic=false;
        return app;
    }
    private void Select(int mask){for(int i=0;i<Ids.Length;i++)_main!.Scps.SetEnabled(Ids[i],(mask&(1<<i))!=0);}
    private void Advance(double seconds)
    {
        for(int i=0;i<(int)Math.Ceiling(seconds/.02);i++)
        {
            _main!.Objects!.Advance(.02);_main.Plague?.State.TickDoctor(.02);
            _main.Companion.Advance(.02,_main.Companion.State.Foot);
            _main.Systems!.ApplyReactions(.02);
        }
    }
    private void StoreChecks()
    {
        string file=Path.Combine(_output,"settings.json");
        File.WriteAllText(file,"{\"version\":1,\"enabled\":{\"scp-049\":false,\"future-scp\":false,\"scp-999\":\"bad\"}}");
        var store=new ScpSettingsStore(file);var loaded=store.Load();
        Check(!loaded["scp-049"]&&!loaded.ContainsKey("scp-999")&&!loaded.ContainsKey("scp-173"),"Missing and malformed entries leave catalog defaults available");
        Check(store.Save(new Dictionary<string,bool>{{"scp-999",true}})&&new ScpSettingsStore(file).Load()["future-scp"]==false,"Unknown IDs survive atomic saving without creating actors");
        string damaged=Path.Combine(_output,"damaged.json");File.WriteAllText(damaged,"not-json");store=new(damaged);store.Load();store.Save(loaded);
        Check(File.ReadAllText(damaged)=="not-json"&&File.Exists(damaged+".recovered.json"),"Corrupt original is preserved with a separate recovery file");
        Check(new ScpSettingsStore(damaged).Load()["scp-049"]==false,"Subsequent launch restores recovery preferences while preserving the damaged original");
        string future=Path.Combine(_output,"future.json");File.WriteAllText(future,"{\"version\":99,\"enabled\":{}}");store=new(future);store.Load();store.Save(loaded);
        Check(File.ReadAllText(future).Contains("99")&&File.Exists(future+".recovered.json"),"Newer schema is not overwritten");
        string obstacle=Path.Combine(_output,"not-a-directory");File.WriteAllText(obstacle,"fixture");store=new(Path.Combine(obstacle,"settings.json"));
        Check(!store.Save(loaded)&&store.Status.Contains("실패"),"Write failure is reported without invalidating session selection");
        var fake=new ProbeModule();var defs=new[]{new ScpDefinition("future","Future","fixture",false,_=>fake)};
        bool locked=false;var manager=new ScpActivationManager(defs,new Dictionary<string,bool>(),()=>locked);
        Check(!manager.IsActive("future"),"New registration uses its own missing-key default");
        manager.SetEnabled("future",true);locked=true;manager.RefreshLockdown();locked=false;manager.RefreshLockdown();
        Check(fake.Active&&manager.Settings["future"],"A new module participates in toggles, lockdown and persistence without a switch");
        var panel=new ScpOptionsPanel{Manager=manager,Store=new(null)};AddChild(panel);Check(panel.Toggles.ContainsKey("future"),"New registration generates its UI row");panel.QueueFree();
        manager.Shutdown();manager.Shutdown();Check(fake.Stops==1&&!manager.SetEnabled("future",true),"Shutdown is idempotent and rejects later changes");
        bool duplicate=false;try{new ScpActivationManager(new[]{defs[0],defs[0]},new Dictionary<string,bool>(),()=>false);}catch(ArgumentException){duplicate=true;}
        Check(duplicate,"Duplicate stable IDs are rejected before modules are created");
    }
    private void Combinations()
    {
        var app=_main!;var sys=app.Systems!;var world=app.Objects!.State;
        for(int mask=0;mask<8;mask++)
        {
            Select(mask);bool doctor=(mask&2)!=0,statue=(mask&4)!=0;
            Check(app.Companion.State.Visible==((mask&1)!=0)&&app.Plague!.State.Visible==doctor&&app.Statue!.State.Visible==statue&&world.Visible==(doctor||statue),$"Combination {mask}: independent bodies and shared world");
            double dc=app.Plague!.State.Clock,wc=world.Clock;float stress=app.Statue!.State.Stress.Value;
            sys.Accept(new(90,90,90,false),sys.State.Generation);Advance(.1);
            Check((app.Plague!.State.Clock>dc)==doctor&&(world.Clock>wc)==(doctor||statue)&&(!statue?app.Statue!.State.Stress.Value==stress:true),$"Combination {mask}: only active clocks and stress progress");
            sys.SetLocked(true);var wanted=app.Scps.Settings;dc=app.Plague!.State.Clock;wc=world.Clock;Advance(.2);
            Check(app.Scps.Entries.All(e=>!e.Active)&&dc==app.Plague!.State.Clock&&wc==world.Clock,$"Combination {mask}: lockdown freezes clocks");
            sys.SetLocked(false);Check(app.Scps.Entries.All(e=>e.Active==wanted[e.Definition.Id]),$"Combination {mask}: resume honors individual preferences");
        }
        Select(7);sys.SetLocked(true);app.Scps.SetEnabled("scp-049",false);app.Scps.SetEnabled("scp-173",false);app.Scps.SetEnabled("scp-173",true);
        Check(!app.Statue!.State.Visible&&!app.Plague!.State.Visible,"Changing preferences during lockdown does not expose actors");
        sys.SetLocked(false);Check(app.Statue!.State.Visible&&!app.Plague!.State.Visible,"Resume applies the final locked-time selection");
        float held=app.Plague!.State.DoctorStress.Value;app.Stress.Relieve(app.Plague!.State.Doctor.Body.Foot,100000,20);sys.ApplyReactions(.1);
        Check(app.Plague!.State.DoctorStress.Value==held,"Disabled stress excludes both relief and metric updates");
        app.Plague!.State.DoctorStress.Set(100);app.Statue!.State.Stress.Set(0);Check(app.Scps.EffectSource==null,"Inactive high stress cannot request external effects");
        app.Statue!.State.Stress.Set(90);Check(app.Scps.EffectSource?.Definition.Id=="scp-173","Active effect requests retain their SCP owner");
        app.Scps.SetEnabled("scp-173",false);Check(app.Scps.EffectSource==null&&!sys.Shake.Active,"Disabling effect owner clears eligibility");
    }
    private async Task IconGenerationChecks()
    {
        var pending=new TaskCompletionSource<DesktopIconImage[]>();int reads=0;
        var pixels=new[]{new DesktopIconImage("owned",1,1,"fixture",new byte[]{255,255,255,255})};
        var icons=new SiteDesktop.Overlay.DesktopIconAppearance{ReadImagesAsync=_=>++reads==1?pending.Task:Task.FromResult(pixels)};
        AddChild(icons);await Frames(2);icons.SetPaused(true);pending.SetResult(pixels);await Frames(3);
        Check(icons.CatalogCount==0&&icons.Choose()==null,"Late icon result after pause cannot populate the catalog");
        icons.SetPaused(false);await Frames(4);
        Check(icons.CatalogCount==1&&icons.Choose()=="owned","Resume accepts a fresh icon generation");
        icons.Shutdown();Check(icons.CachedCount==0&&icons.Stopped,"Shared icon shutdown releases cached textures");icons.QueueFree();
    }
    private void Interactions()
    {
        var app=_main!;var c=app.Plague!;var world=app.Objects!.State;Select(6);
        foreach(int id in world.Objects.Keys.ToArray())world.Collect(id);
        c.ResetDoctor();int target=world.SpawnAt(c.State.Doctor.Body.Foot+new Vector2(20,0))!.Value;
        for(int i=0;i<40&&c.State.Phase!=DoctorPhase.Treating;i++)c.Advance(.02);
        Check(c.State.Phase==DoctorPhase.Treating,"Treatment fixture uses the actual doctor and shared object");
        double treatment=c.State.TreatmentAge;app.Systems!.SetLocked(true);Advance(.2);app.Systems.SetLocked(false);
        Check(c.State.TreatmentAge==treatment&&c.State.TargetId==target,"Global lockdown preserves treatment progress and reservation");
        app.Systems.SetLocked(true);app.Scps.SetEnabled("scp-049",false);
        Check(c.State.TargetId==null&&c.State.TreatmentAge==0&&world.Objects[target].ClaimedBy==null&&!c.State.CompleteTreatment(target,PlagueState.DoctorId),"Individual OFF during lockdown cancels treatment and rejects late completion");
        app.Systems.SetLocked(false);Check(world.TryClaim(target,StatueState.Id)&&world.Break(target,StatueState.Id),"Statue can claim and break the released shared target with doctor OFF");
        Advance(1);Check(!world.Objects.ContainsKey(target),"Shared object lifetime runs independently of the disabled doctor");
        app.Scps.SetEnabled("scp-049",true);c.ResetDoctor();target=world.SpawnAt(c.State.Doctor.Body.Foot+new Vector2(20,0))!.Value;
        for(int i=0;i<500&&world.Objects[target].Kind!=DummyKind.Zombie;i++)c.Advance(.02);
        var zombie=world.Objects[target];Check(zombie.Kind==DummyKind.Zombie&&zombie.OwnerId==PlagueState.DoctorId,"Transformation records owner identity on the existing object");
        app.Scps.SetEnabled("scp-049",false);double age=zombie.Age;int frame=zombie.ZombieAnimation!.Frame;double zc=world.ZombieClock;
        int data=world.SpawnAt(world.Screens[0].Usable.Position+new Vector2(300,300))!.Value;Advance(1);
        Check(zombie.Age==age&&world.ZombieClock==zc&&zombie.ZombieAnimation.Frame==frame&&world.Objects[data].Age>0&&!world.Collect(target),"Owned zombies freeze while ordinary shared data advances and inactive input is rejected");
        Check(zombie.MutationAge==null&&!c.Presentation.Auras().Any()&&!c.Presentation.Disappearing.Any(),"Individual OFF clears transient mutation and disappearance effects");
        app.Scps.SetEnabled("scp-049",true);Advance(.02);Check(zombie.Age>age&&zombie.Age<age+.05,"Zombie resumes remaining life without catch-up");
        world.BeginObjectDrag(target,zombie.Foot);app.Scps.SetEnabled("scp-049",false);Check(world.DraggedObject==null,"Owner OFF releases zombie drag");
        Select(7);app.Companion.State.BeginDrag(app.Companion.State.Foot);app.Statue!.State.BeginDrag(app.Statue.State.Physics.Body.Foot);c.State.BeginDoctorDrag(c.State.Doctor.Body.Foot);
        Select(0);Check(app.Companion.State.Gesture.ToString()!="Drag"&&!app.Statue.State.Dragging&&c.State.Phase!=DoctorPhase.Dragging,"Individual OFF cancels all three body drag states");
        Select(4);world.TryClaim(data,StatueState.Id);app.Scps.SetEnabled("scp-173",false);
        Check(world.Objects[data].ClaimedBy==null&&!world.Break(data,StatueState.Id),"Disabling an owner releases its shared reservation and rejects a late attack");
        Select(7);int registrations=app.Stress.Targets.Count;
        for(int repeat=0;repeat<20;repeat++){Select(0);app.Systems.SetLocked(true);Select(7);app.Systems.SetLocked(false);}
        Check(app.Stress.Targets.Count==registrations&&registrations==2,"Twenty transitions do not duplicate stress registrations");
        Check(app.Scps.Entries.Count==3&&app.GetChildren().Count(n=>n is SiteDesktop.Overlay.Scp999Controller)==1,"Twenty transitions reuse module instances");
        Select(0);Check(!app.Companion.KeyboardMonitoring&&!world.Visible&&(app.Systems.Tray?.Available==true||DisplayServer.GetName()=="headless"),"All OFF retains application access and stops input/world");
    }
    public override async void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();for(int i=0;i+1<args.Length;i++)if(args[i]=="--output")_output=args[++i];
            if(_output==""||File.Exists(Path.Combine(_output,"results.json")))throw new InvalidOperationException("Fresh evidence directory required");Directory.CreateDirectory(_output);
            StoreChecks();await IconGenerationChecks();_main=Create();Combinations();Interactions();bool native=DisplayServer.GetName()!="headless";
            if(native)
            {
                await Frames(4);Check(!_main.Companion.BodyWindow!.Visible&&!_main.Statue!.Actor!.Visible&&!_main.Plague!.DoctorWindow!.Visible&&_main.Objects!.ObjectWindows.Values.All(w=>!w.Visible),"All OFF removes native body and object surfaces");
                Select(4);await Frames(4);Check(_main.Statue!.Actor!.Visible&&!_main.Plague!.DoctorWindow!.Visible,"Native statue remains visible with doctor disabled");
                _main.ShowControls();await Frames(15);using(var png=_main.GetWindow().GetTexture().GetImage())png.SavePng(Path.Combine(_output,"controls.png"));
                _main.OptionsPanel.Toggles["scp-999"].ButtonPressed=true;Check(_main.Scps.IsActive("scp-999"),"Actual UI toggle signal uses manager command");
                _main.HideControls();Check(!WindowsDesktop.NativeVisible(_main.GetWindow())&&_main.Scps.IsActive("scp-999"),"Control hiding preserves individual preferences and activity");
                using(var receiver=new NativeInputFixture(_output,decorated:true))
                {
                    for(int i=0;i<600&&!receiver.Ready;i++)await Frames(1);
                    Check(receiver.Ready,"Owned external-effect receiver is ready");
                    bool focused=false;GD.Print("ACTIVATION_RECEIVER_FOCUS_WAIT");for(int i=0;i<180&&!focused;i++){focused=args.Contains("--native-input")?receiver.Focus():receiver.FocusWithoutInput();await Frames(1);}
                    Check(focused,"Owned receiver acquires foreground using the requested input policy");
                    _main.Statue!.State.Stress.Set(90);_main.Systems!.SetStrong(true);
                    Check(_main.Systems.RequestStressEffect(),"Active SCP starts an effect on the owned receiver");
                    int moves=_main.Systems.Shake.Moves,restores=_main.Systems.Shake.Restores;_main.Systems.Shake.Tick(.06);
                    Check(_main.Systems.Shake.Moves>moves,"Owned external window receives the bounded effect");
                    _main.Scps.SetEnabled("scp-173",false);
                    Check(!_main.Systems.Shake.Active&&_main.Systems.Shake.Restores==restores+1,"Disabling source immediately cancels and restores the owned external window");
                    _main.Systems.SetStrong(false);
                }
            }
            string settings=Path.Combine(_output,"restart.json");var store=new ScpSettingsStore(settings);store.Save(new Dictionary<string,bool>{{"scp-999",false},{"scp-049",false},{"scp-173",true}});
            _main.QueueFree();await Frames(4);_main=Create(settings);
            Check(!_main.Companion.State.Visible&&!_main.Plague!.State.Visible&&_main.Statue!.State.Visible,"Fresh application restores preferences before first actor publication");
            if(native)Check(!_main.Companion.BodyWindow!.Visible&&!_main.Plague!.DoctorWindow!.Visible,"Disabled startup actors never publish visible native windows");
            _main.Scps.SetEnabled("scp-173",false);Check(new ScpSettingsStore(settings).Load().Values.All(v=>!v),"Product UI command persists changes through settings store");
            _main.QueueFree();await Frames(4);_main=Create(doctor:false);
            Check(_main.Plague==null&&_main.Statue!=null&&_main.Objects!=null,"Statue and shared world initialize without constructing SCP-049");
            if(args.Contains("--interactive"))
            {
                _main.QueueFree();await Frames(4);_main=Create(settings);_main.ShowControls();GD.Print("ACTIVATION_INTERACTIVE_READY");
                ulong deadline=Time.GetTicksMsec()+240000;
                while(Time.GetTicksMsec()<deadline&&!File.Exists(Path.Combine(_output,"finish-ui-test")))await Frames(1);
                File.WriteAllText(Path.Combine(_output,"ui-final-state.json"),JsonSerializer.Serialize(new{settings=_main.Scps.Settings,controls=WindowsDesktop.NativeVisible(_main.GetWindow()),keyboard=_main.Companion.KeyboardMonitoring}));
            }
            File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new{passed=true,native,checks=_checks},new JsonSerializerOptions{WriteIndented=true}));
            GD.Print("ACTIVATION_PASS");_main.RequestExit();
        }
        catch(Exception ex){GD.PushError("ACTIVATION_FAILED "+ex);_main?.RequestExit();GetTree().Quit(1);}
    }
    private sealed class ProbeModule : IScpModule
    {
        public bool Active;public int Stops;public string Status=>"fixture";public float Stress=>0;
        public void SetActivity(bool active,ActivityReason reason){Active=active;}
        public void ApplyReactions(SystemOptions system,double delta){}
        public void Reset(){}
        public void Shutdown(){Stops++;}
    }
}
