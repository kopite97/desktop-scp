using Godot;
using SiteDesktop.App;
using SiteDesktop.Desktop;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Platforms;
using SiteDesktop.Simulation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;

public partial class DoctorArtSmoke : Node
{
    private Main? _main;private string _output="";
    private readonly List<string> _checks=new();private readonly HashSet<string> _seen=new();
    private object? _cost;
    private static readonly DesktopScreen Screen=new(0,new(-1920,1061,1920,1080),new(-1920,1061,1920,1032),96);
    private void Check(bool condition,string name){if(!condition)throw new InvalidOperationException(name);_checks.Add(name);GD.Print("DOCTOR_ART_CHECK "+name);}
    private async Task Frames(int n){for(int i=0;i<n;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private PlagueState Create()=>new(new[]{Screen},new StressState()){AutoSpawn=false};
    private void Step(PlagueState s,double seconds)
    {for(int i=0;i<(int)Math.Ceiling(seconds/.02);i++){s.Tick(.02);_seen.Add(s.Animation.Tag);}}
    private void Logic()
    {
        string product=ProjectSettings.GlobalizePath(DoctorAnimation.AssetPath);
        string source=Path.GetFullPath("../Art/assets/characters/Scp049/revisions/r001/export",ProjectSettings.GlobalizePath("res://"));
        foreach(string file in new[]{"Scp049.png","Scp049.contract.json"})Check(SHA256.HashData(File.ReadAllBytes(Path.Combine(product,file))).SequenceEqual(SHA256.HashData(File.ReadAllBytes(Path.Combine(source,file)))),"Approved r001 hash matches "+file);
        var s=Create();Step(s,1);var set=s.Animation.Set;
        Check(set.Clips.Count==10&&set.Frames.Length==42&&set.Cell==new Vector2I(64,64),"Contract has ten tags and 42 fixed 64px cells");
        foreach(var clip in set.Clips.Values)
        {
            double age=0;for(int i=clip.From;i<=clip.To;i++)
            {Check(set.FrameAt(clip.Name,age+set.Frames[i].Seconds/2)==i,$"{clip.Name} selects frame {i} for its authored duration");age+=set.Frames[i].Seconds;}
            Check(set.FrameAt(clip.Name,set.Duration(clip.Name)+.001)==(clip.Loop?clip.From:clip.To),clip.Name+" loop/hold matches contract");
        }
        s.SpawnAt(s.Doctor.Body.Foot+new Vector2(180,0));Step(s,6);
        Check(_seen.IsSupersetOf(new[]{"idle","move","treat_start","treat_loop","treat_end"})&&s.Transformations==1,"Actual seek/treatment produces start-loop-end without changing three-second game rule");
        var age0=s.Animation.Age;s.SetVisible(false);Step(s,1);Check(s.Animation.Age==age0,"Hidden animation time freezes");s.SetVisible(true);s.Shutdown();
        s=Create();int id=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(-35,0))!.Value;Step(s,.3);
        Check(s.Animation.Flipped&&s.Animation.Tag=="treat_start","Surgery faces left-side target even when velocity is zero");
        var anchor=s.Animation.ToolAnchor;var f=s.Animation.Frame;
        s.Collect(id);Check(s.Animation.Tag is not ("treat_start" or "treat_loop"),"Collection cancels surgery animation immediately");
        id=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(35,0))!.Value;Step(s,.3);
        Check(!s.Animation.Flipped,"Right-side target restores original orientation");
        Check(Math.Abs(anchor.X+s.Animation.ToolAnchor.X-128)<.01&&f==s.Animation.Frame,"Treatment anchors mirror about the fixed pivot");
        s.BeginDoctorDrag(s.Doctor.Body.Foot);Check(s.Animation.Tag=="fall"&&s.TargetId==null,"Drag interrupts treatment immediately");s.Drag(s.Doctor.Body.Foot-new Vector2(0,140));s.EndDrag();Step(s,1);Check(_seen.Contains("fall")&&_seen.Contains("land"),"Actual drop and landing map to fall and land");s.Shutdown();
        s=Create();s.TreatmentRate=1.5f;s.SpawnAt(s.Doctor.Body.Foot+new Vector2(35,0));Step(s,2.3);
        Check(s.Transformations==1&&s.Animation.Tag=="treat_end","Overload accelerates surgery without delaying logical completion for art");s.Shutdown();
        s=Create();var w=new WindowObservation("A",new(-1700,1661,500,240));s.Observe(new[]{w});s.Doctor.Place(new(-1450,1500));Step(s,.9);
        s.Observe(new[]{w with{Bounds=new(-1660,1641,500,240)}});Step(s,.12);Check(s.Animation.Tag=="balance","Moving support selects balance");Step(s,.3);Check(s.Animation.Tag=="balance_recover","Settling support selects balance recovery");Step(s,.6);s.Shutdown();
        s=Create();var upper=new WindowObservation("B",new(-1100,1561,450,240));s.Observe(new[]{upper,w});s.Doctor.Place(new(-1280,1500));s.SpawnAt(new(-1030,1400));Step(s,8);
        Check(s.Doctor.Body.Jumps>0&&_seen.Contains("takeoff"),"Actual adjacent-platform jump selects takeoff");
        Check(_seen.IsSupersetOf(set.Clips.Keys),"All ten tags are reached by actual game/physics transitions");s.Shutdown();
    }
    public override async void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();for(int i=0;i+1<args.Length;i++)if(args[i]=="--output")_output=args[++i];
            if(_output==""||File.Exists(Path.Combine(_output,"results.json")))throw new InvalidOperationException("New evidence required");Directory.CreateDirectory(_output);
            Logic();bool native=DisplayServer.GetName()!="headless",input=args.Contains("--native-input");
            if(native)await Native(input);
            File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new{passed=true,native,nativeInput=input,checks=_checks,tags=_seen,cost=_cost},new JsonSerializerOptions{WriteIndented=true}));
            GD.Print("DOCTOR_ART_PASS");if(_main!=null)_main.RequestExit();else GetTree().Quit();
        }
        catch(Exception ex){GD.PushError("DOCTOR_ART_FAILED "+ex);_main?.RequestExit();GetTree().Quit(1);}
    }
    private async Task Native(bool input)
    {
        _main=GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();_main.PersistScpSettings=false;_main.EnableSystemOptions=false;_main.EnableStatue=false;AddChild(_main);
        _main.Companion.SetVisible(false);var c=_main.Plague!;c.Automatic=false;c.State.AutoSpawn=false;
        foreach(int id in c.State.Objects.Keys.ToArray())c.State.Collect(id);
        var w=c.DoctorWindow!;var art=w.DoctorArt!;var s=c.State;
        using var source=Image.LoadFromFile(ProjectSettings.GlobalizePath(DoctorAnimation.AssetPath+"Scp049.png"));
        int CheckPixels(Image image,int frame,bool flip)
        {
            int bad=0;for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                var expected=source.GetPixel(frame%7*64+(flip?63-x/2:x/2),frame/7*64+y/2);var actual=image.GetPixel(x,y);
                if(Math.Abs(expected.A-actual.A)>.01||(expected.A>0&&(Math.Abs(expected.R-actual.R)>.01||Math.Abs(expected.G-actual.G)>.01||Math.Abs(expected.B-actual.B)>.01)))bad++;
                if(art.Hit(new(x+.5f,y+.5f))!=(expected.A>=16/255f))bad++;
            }
            return bad;
        }
        for(int frame=0;frame<42;frame++)foreach(bool flip in new[]{false,true})
        {
            art.Frame=frame;art.FlipH=flip;await Frames(2);using var image=w.GetTexture().GetImage();
            Check(CheckPixels(image,frame,flip)==0,$"Native frame {frame}, flip {flip}: exact color/alpha/nearest and click mask");
            if(frame is 0 or 17 or 23&&!flip)image.SavePng(Path.Combine(_output,$"frame-{frame:00}.png"));
        }
        art.Frame=0;art.FlipH=false;
        var ambient=new CanvasModulate{Color=new(.12f,.18f,.3f)};w.AddChild(ambient);
        using var white=Image.CreateEmpty(128,128,false,Image.Format.Rgba8);white.Fill(Colors.White);
        using var lightTexture=ImageTexture.CreateFromImage(white);var light=new PointLight2D{Texture=lightTexture,Position=new(64,64),Energy=3,Color=new(1,.4f,.1f)};w.AddChild(light);
        await Frames(3);using(var image=w.GetTexture().GetImage())Check(CheckPixels(image,0,false)==0,"Night CanvasModulate and warm Light2D do not alter approved unshaded pixels");
        var process=Process.GetCurrentProcess();double cpu=process.TotalProcessorTime.TotalMilliseconds;long memory=process.WorkingSet64;var timer=Stopwatch.StartNew();await Frames(60);process.Refresh();
        double litCpu=process.TotalProcessorTime.TotalMilliseconds-cpu,litSeconds=timer.Elapsed.TotalSeconds;
        light.Enabled=false;ambient.Color=Colors.White;await Frames(3);cpu=process.TotalProcessorTime.TotalMilliseconds;timer.Restart();await Frames(60);process.Refresh();
        _cost=new{windowPixels=128*128,litSeconds,litCpuMilliseconds=litCpu,neutralSeconds=timer.Elapsed.TotalSeconds,neutralCpuMilliseconds=process.TotalProcessorTime.TotalMilliseconds-cpu,workingSetStart=memory,workingSetEnd=process.WorkingSet64,gpu="not measured",longRun="not measured"};
        light.QueueFree();ambient.QueueFree();await Frames(3);c.Publish();
        Check(WindowsDesktop.GetBounds(w).Position==(Vector2I)(s.Doctor.Body.Foot-new Vector2(64,112)).Round(),"Native LG physical position keeps approved pivot at the physics foot");
        s.SpawnAt(s.Doctor.Body.Foot+new Vector2(35,0));for(int i=0;i<190;i++){c.Advance(.02);await Frames(1);}
        Check(s.Transformations==1,"Actual native doctor completes treatment while approved sprite plays");
        var anchor=s.Doctor.Body.Foot;c.SetVisible(false);await Frames(3);Check(!w.Visible,"Hide removes approved body window");c.SetVisible(true);await Frames(3);Check(w.Visible&&s.Doctor.Body.Foot==anchor,"Restore preserves approved body position");
        if(input)await Input(c);
        c.Shutdown();await Frames(3);Check(c.DoctorWindow==null&&c.State.Stopped,"Shutdown frees approved doctor window and state");
    }
    private async Task Input(PlagueController c)
    {
        foreach(int id in c.State.Objects.Keys.ToArray())c.State.Collect(id);c.State.ResetDoctor();c.Publish();
        using var receiver=new NativeInputFixture(_output);
        for(int i=0;i<600&&!receiver.Ready;i++)await Frames(1);Check(receiver.Ready,"Owned LG input receiver ready");
        bool focused=false;for(int i=0;i<60&&!focused;i++){focused=receiver.Focus();await Frames(1);}Check(focused,"Receiver owns foreground before synthetic input");
        var w=c.DoctorWindow!;var s=c.State;Vector2 top=WindowsDesktop.GetBounds(w).Position;
        receiver.Move(top+new Vector2(2,2));c.Publish();await Frames(6);var before=receiver.Counts();receiver.Click();await Frames(6);var after=receiver.Counts();
        Check(after.Down==before.Down+1&&after.Up==before.Up+1,"Approved sprite transparent corner forwards real click across processes");
        Vector2 opaque=Vector2.Zero;bool found=false;
        for(int y=50;y<108&&!found;y+=2)for(int x=30;x<98;x+=2)if(w.DoctorArt!.Hit(new(x,y))){opaque=new(x,y);found=true;break;}
        Check(found,"An authored opaque body pixel is available for drag");
        receiver.Move(top+opaque);c.Publish();await Frames(6);var start=s.Doctor.Body.Foot;
        _main!.Companion.AutomaticInput=false;_main.Companion.State.Physics.Place(start+new Vector2(100,-100));_main.Companion.SetVisible(true);
        int batches=_main.Stack.AppliedBatches;
        c.Automatic=true;receiver.Press(true);await Frames(6);receiver.Move(top+opaque+new Vector2(100,-100));await Frames(6);
        Check(s.Phase==DoctorPhase.Dragging&&s.Doctor.Body.Foot.DistanceTo(start+new Vector2(100,-100))<2,"Real right drag follows approved body alpha island");
        Check(_main.Stack.AppliedBatches-batches<=4,"Doctor overlap drag uses bounded common stack updates without per-frame raising");
        receiver.Release();await Frames(3);c.Automatic=false;Check(s.Phase!=DoctorPhase.Dragging,"Real right release ends approved body drag");
        _main.Companion.SetVisible(false);
        c.SetVisible(false);receiver.Move(top+opaque);await Frames(4);before=receiver.Counts();receiver.Click();await Frames(4);after=receiver.Counts();
        Check(after.Down==before.Down+1&&after.Up==before.Up+1,"Hidden approved body leaves no native click blocker");c.SetVisible(true);
    }
}
