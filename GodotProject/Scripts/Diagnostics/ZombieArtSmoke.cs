using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
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

public partial class ZombieArtSmoke : Node
{
    private string _output="";
    private readonly List<string> _checks=new();
    private PlagueController? _controller;
    private DesktopObjectController? _objects;
    private DesktopIconAppearance? _icons;
    private OverlayStackCoordinator? _stack;
    private DesktopIconImage[] _catalog=Array.Empty<DesktopIconImage>();
    private object? _cost;
    private static readonly DesktopScreen Screen=new(0,new(-1920,1061,1920,1080),new(-1920,1061,1920,1032),96);
    private void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);_checks.Add(name);GD.Print("ZOMBIE_ART_CHECK "+name);}
    private async Task Frames(int n){for(int i=0;i<n;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private static void Step(PlagueState s,PlaguePresentation p,int n){for(int i=0;i<n;i++){s.Tick(.02);p.Sync();}}
    private PlagueState State(PlagueSettings? settings=null)=>new(new[]{Screen},new StressState(),settings){AutoSpawn=false};
    private int Transform(PlagueState s,PlaguePresentation p)
    {
        int id=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(35,0))!.Value;
        for(int i=0;i<230&&s.Objects[id].Kind!=DummyKind.Zombie;i++)Step(s,p,1);
        Check(s.Objects[id].Kind==DummyKind.Zombie,"Game treatment actually transforms target "+id);return id;
    }
    private void Logic()
    {
        VerifyForwardGait();
        var hashes=new List<object>();
        foreach(var asset in new[]{("characters","Scp0492"),("vfx","Scp049Mutation")})
        {
            string source=Path.GetFullPath($"../Art/assets/{asset.Item1}/{asset.Item2}/revisions/r001/export",ProjectSettings.GlobalizePath("res://"));
            foreach(string file in Directory.GetFiles(source))
            {string name=Path.GetFileName(file);var bytes=File.ReadAllBytes(file);var product=File.ReadAllBytes(ProjectSettings.GlobalizePath($"res://Assets/Sprites/{asset.Item1}/{asset.Item2}/{name}"));string hash=Convert.ToHexString(SHA256.HashData(bytes));Check(bytes.SequenceEqual(product),"Approved r001 bytes: "+name);hashes.Add(new{name,sha256=hash});}
        }
        File.WriteAllText(Path.Combine(_output,"asset-hashes.json"),JsonSerializer.Serialize(hashes,new JsonSerializerOptions{WriteIndented=true}));
        foreach(var set in new[]{ZombieAnimation.Set,PlaguePresentation.EffectSet})foreach(var clip in set.Clips.Values)
        {
            double start=0;for(int f=clip.From;f<=clip.To;f++){Check(set.FrameAt(clip.Name,start+set.Frames[f].Seconds/2)==f,$"Authored {clip.Name} frame{f} duration/index");start+=set.Frames[f].Seconds;}
            Check(set.FrameAt(clip.Name,start+.001)==(clip.Loop?clip.From:clip.To),"Authored loop/terminal: "+clip.Name);
        }
        Check(Math.Abs(PlaguePresentation.EffectSet.Duration("transform")-.8)<1e-8,"Transform includes its transparent tail in800ms");
        foreach(float rate in new[]{1f,1.5f})
        {
            var s=State();using var p=new PlaguePresentation(s);s.TreatmentRate=rate;
            int id=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(35,0))!.Value;
            while(s.Phase!=DoctorPhase.Treating)s.Tick(.02);
            double start=s.Clock;Step(s,p,24);var aura=p.Auras().Single();
            Check(Math.Abs(aura.Age-.48)<1e-7&&aura.Tag=="treat",$"Treat clock stays unscaled at rate{rate}");
            while(s.Objects[id].Kind!=DummyKind.Zombie&&s.Clock-start<4)Step(s,p,1);
            Check(Math.Abs((s.Clock-start)-3/rate)<.05,$"Game success stays approximately{3/rate}s");
            var item=s.Objects[id];Check(item.ZombieAnimation!.Tag=="spawn"&&item.ZombieAnimation.Age==0&&p.Auras().Single().Age==0,"Success starts spawn and transform at the same zero");
            var before=item.Foot;Step(s,p,10);Check(item.Foot!=before&&item.ZombieAnimation.Tag=="spawn","Spawn does not freeze game movement");
            s.BeginObjectDrag(id,item.Foot);Check(item.ZombieAnimation.Tag=="fall"&&!p.Auras().Any(a=>a.Tag=="treat"),"Dragging interrupts spawn without restarting treatment");
            s.Drag(item.Foot-new Vector2(0,100));s.EndDrag();bool landed=false;for(int i=0;i<100;i++){Step(s,p,1);landed|=item.ZombieAnimation.Tag=="land";}
            Check(landed,"Real drop selects authored landing");
            double age=item.ZombieAnimation.Age;bool flip=item.ZombieAnimation.Flipped;
            item.Physics.Body.Velocity=new(flip?40:-40,0);item.ZombieAnimation.Update(item,s.Clock);
            Check(item.ZombieAnimation.Flipped!=flip&&item.ZombieAnimation.Age==age,"Facing change preserves animation phase");
            int collected=s.Collected;Check(s.Collect(id)&&!s.Collect(id)&&s.Collected==collected+1&&!s.Objects.ContainsKey(id),"Collection removes game identity and rewards exactly once");
            Check(p.Disappearing.Count==1&&p.Auras().All(a=>a.Id!=id),"Collection preserves only a decorative snapshot");
            Step(s,p,8);double snapshotAge=p.Disappearing[id].Age(s.Clock);s.SetVisible(false);Step(s,p,50);
            Check(p.Disappearing[id].Age(s.Clock)==snapshotAge,"Lockdown freezes disappearance clock");s.SetVisible(true);
            Step(s,p,9);Check(p.Disappearing.Count==1&&ZombieAnimation.Set.Frames[p.Disappearing[id].Frame(s.Clock)].BodyAlpha==0,"Transparent collect tail holds before cleanup");
            Step(s,p,1);Check(p.Disappearing.Count==0,"Collect cleans up at360ms");s.Shutdown();
        }
        {
            var s=State();using var p=new PlaguePresentation(s);int id=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(35,0))!.Value;Step(s,p,20);
            Check(p.Auras().Single().Tag=="treat","Actual Treating produces aura");s.BeginDoctorDrag(s.Doctor.Body.Foot);
            Check(!p.Auras().Any()&&s.Objects[id].ZombieAnimation==null,"Cancellation cannot produce transform/feet");s.EndDrag();
            for(int i=0;i<50&&s.Phase!=DoctorPhase.Treating;i++)Step(s,p,1);
            Check(p.Auras().Single().Age==0,"Treatment restart begins at frame zero");s.Collect(id);Check(p.Disappearing.Count==0&&!p.Auras().Any(),"Normal data collection has no zombie disappearance");s.Shutdown();
        }
        {
            var s=State();using var p=new PlaguePresentation(s);int id=Transform(s,p);s.Objects[id].Age=s.Settings.ZombieLifetime-.01;Step(s,p,1);
            Check(!s.Objects.ContainsKey(id)&&s.Expired==1&&p.Disappearing[id].Tag=="expire","Expiry starts only the authored expire snapshot");
            Step(s,p,32);Check(p.Disappearing.Count==1&&ZombieAnimation.Set.Frames[p.Disappearing[id].Frame(s.Clock)].BodyAlpha==0,"Expire retains transparent tail through640ms");Step(s,p,1);Check(p.Disappearing.Count==0,"Expire cleans up at660ms");
            id=Transform(s,p);s.Collect(id);p.ReplaceScreens();Check(p.Disappearing.Count==0,"Monitor change discards detached snapshots");s.Shutdown();p.Sync();Check(!p.Disappearing.Any()&&!p.Auras().Any(),"Shutdown clears all presentation identity");
        }
    }
    private void VerifyForwardGait()
    {
        // Read the actual sole pixels, not a duplicate of the frame lookup.
        // While planted, their local travel must oppose world movement.
        foreach(string side in new[]{"left","right"})
        {
            using var sheet=Image.LoadFromFile(ProjectSettings.GlobalizePath(ZombieAnimation.AssetPath+$"Scp0492_{side}-foot.png"));
            foreach(bool flip in new[]{false,true})
            {
                double? previous=null;int last=-1,backwardSteps=0;bool forwardSlip=false;
                for(double time=0;time<ZombieAnimation.Set.Duration("move")*2;time+=.005)
                {
                    int frame=ZombieAnimation.FrameAt("move",time);if(frame==last)continue;last=frame;
                    var sole=Enumerable.Range(0,32).Where(x=>sheet.GetPixel(frame%6*32+x,frame/6*32+31).A>=16/255f).ToArray();
                    if(sole.Length==0){previous=null;continue;}
                    double center=sole.Average(),position=flip?32-center:center;
                    if(previous.HasValue){double alongFacing=(position-previous.Value)*(flip?-1:1);forwardSlip|=alongFacing>0; if(alongFacing<0)backwardSteps++;}
                    previous=position;
                }
                Check(!forwardSlip&&backwardSteps>0,$"{side} foot flip{flip}: planted sole travels opposite facing");
            }
        }
        var clip=ZombieAnimation.Set.Clips["move"];double elapsed=0;
        for(int f=clip.To;f>=clip.From;f--){Check(ZombieAnimation.FrameAt("move",elapsed+ZombieAnimation.Set.Frames[f].Seconds/2)==f,"Corrected gait preserves authored duration frame"+f);elapsed+=ZombieAnimation.Set.Frames[f].Seconds;}
        Check(Math.Abs(elapsed-.84)<1e-7,"Corrected gait keeps840ms period");
    }
    private static DesktopIconImage Fixture(int kind)
    {
        var data=new byte[48*48*4];for(int y=0;y<48;y++)for(int x=0;x<48;x++)
        {
            double radius=(x-23.5)*(x-23.5)+(y-23.5)*(y-23.5);
            bool on=kind switch{0=>x>=4&&x<44&&y>=4&&y<44,1=>radius<21*21,2=>(x>=8&&x<17&&y>=5&&y<44)||(x>=17&&x<40&&y>=25&&y<44),3=>radius<21*21&&radius>10*10,4=>x>=18&&x<30&&y>=18&&y<30,5=>x>=20&&x<28&&y>=4&&y<44,6=>x==24&&y>=4&&y<44,7=>x>=23&&x<25&&y>=4&&y<44,_=>false};
            if(!on)continue;int i=(y*48+x)*4;data[i]=(byte)(90+kind*12);data[i+1]=(byte)(165-kind*8);data[i+2]=195;data[i+3]=255;
        }
        return new(Convert.ToHexString(SHA256.HashData(data)),48,48,"owned-art-fixture",data);
    }
    public override async void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();for(int i=0;i+1<args.Length;i++)if(args[i]=="--output")_output=args[++i];
            if(_output==""||File.Exists(Path.Combine(_output,"results.json")))throw new InvalidOperationException("New evidence required");Directory.CreateDirectory(_output);
            Logic();bool native=DisplayServer.GetName()!="headless",input=args.Contains("--native-input");if(native)await Native(input);
            File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new{passed=true,native,nativeInput=input,checks=_checks,cost=_cost},new JsonSerializerOptions{WriteIndented=true}));
            GD.Print("ZOMBIE_ART_PASS");_controller?.Shutdown();_objects?.Shutdown();_icons?.Shutdown();GetTree().Quit();
        }
        catch(Exception ex){GD.PushError("ZOMBIE_ART_FAILED "+ex);_controller?.Shutdown();_objects?.Shutdown();_icons?.Shutdown();GetTree().Quit(1);}
    }
    private async Task Native(bool input)
    {
        var screen=DesktopPlacement.Select();GetWindow().Position=WindowsDesktop.ToGodotPosition(screen.Usable.Position+new Vector2I(20,20));GetWindow().Size=new(400,100);GetWindow().Title="PLAN-014 art verification";
        _stack=new();AddChild(_stack);_catalog=Enumerable.Range(0,9).Select(Fixture).ToArray();
        _icons=new(){ReadImagesAsync=_=>Task.FromResult(_catalog)};AddChild(_icons);await Frames(4);
        Check(_icons.CatalogCount==9,"Owned fixtures populate the real texture cache");
        _objects=new(){Icons=_icons,Stack=_stack,Automatic=false};AddChild(_objects);
        _controller=new(){Objects=_objects,Stack=_stack,Stress=new()};AddChild(_controller);var c=_controller;var s=c.State;s.AutoSpawn=false;
        foreach(int id in s.Objects.Keys.ToArray())s.Collect(id);
        using var left=Image.LoadFromFile(ProjectSettings.GlobalizePath(ZombieAnimation.AssetPath+"Scp0492_left-foot.png"));
        using var right=Image.LoadFromFile(ProjectSettings.GlobalizePath(ZombieAnimation.AssetPath+"Scp0492_right-foot.png"));
        static Color Over(Color source,Color dest)=>new(source.R*source.A+dest.R*(1-source.A),source.G*source.A+dest.G*(1-source.A),source.B*source.A+dest.B*(1-source.A),source.A+dest.A*(1-source.A));
        static bool Near(Color a,Color b)=>Math.Abs(a.A-b.A)<.012&&Math.Abs(a.R-b.R)<.012&&Math.Abs(a.G-b.G)<.012&&Math.Abs(a.B-b.B)<.012;
        for(int k=0;k<9;k++)
        {
            var pixels=Fixture(k);var icon=_icons.Find(pixels.Id)!;s.ChooseAppearance=()=>pixels.Id;s.ResetDoctor();int id=Transform(s,c.Presentation);c.Publish();
            var item=s.Objects[id];item.Physics.Place(screen.Usable.Position+new Vector2(500+k*105,380));c.Publish();var w=c.ObjectWindows[id];
            if(k==0)
            {
                await Frames(3);
                using var messages=new WindowRedrawProbe(w);
                for(int repeat=0;repeat<40;repeat++)w.Publish(s);
                var observed=new{publishes=40,positionChanges=messages.PositionChanges,backgroundErases=messages.BackgroundErases,nonClientPaints=messages.NonClientPaints};
                File.WriteAllText(Path.Combine(_output,"native-redraw.json"),JsonSerializer.Serialize(observed));
                Check(messages.PositionChanges==0&&messages.BackgroundErases==0&&messages.NonClientPaints==0,"Unchanged Publish does not reset native region or erase window background");
            }
            Check(!icon.HasPixels||(icon.ZombieOffset.Y+icon.VisibleBounds.End.Y==56&&Math.Abs(icon.ZombieOffset.X+icon.VisibleBounds.Position.X+icon.VisibleBounds.Size.X/2f-32)<=.5),"Visible alpha bounds align without resampling fixture"+k);
            for(int f=0;f<30;f++)foreach(bool flip in new[]{false,true})
            {
                w.ZombieArt!.Publish(icon,f,flip);await Frames(2);using var actual=w.GetTexture().GetImage();int bad=0,hits=0;
                for(int y=0;y<64;y++)for(int x=0;x<64;x++)
                {
                    var expect=new Color(0,0,0,0);bool hit=false;
                    if(icon.HasPixels)foreach(var pair in new[]{(left,icon.LeftRoot),(right,icon.RightRoot)})
                    {
                        int sx=(int)Math.Floor((x-pair.Item2)/2.0);sx=flip?16-sx:16+sx;int sy=(int)Math.Floor((y-56)/2.0)+28;
                        if(sx>=0&&sx<32&&sy>=0&&sy<32){var color=pair.Item1.GetPixel(f%6*32+sx,f/6*32+sy);expect=Over(color,expect);hit|=color.A>=16/255f;}
                    }
                    int ix=x-(int)icon.ZombieOffset.X,iy=y-(int)icon.ZombieOffset.Y;
                    if(ix>=0&&iy>=0&&ix<48&&iy<48){int at=(iy*48+ix)*4;var body=new Color(pixels.Rgba[at]/255f,pixels.Rgba[at+1]/255f,pixels.Rgba[at+2]/255f,pixels.Rgba[at+3]/255f);expect=Over(body,expect);hit|=body.A>=16/255f;}
                    if(!Near(actual.GetPixel(x,y),expect))bad++;if(w.Hit(new(x+.5f,y+.5f))!=hit)hits++;
                }
                Check(bad==0&&hits==0,$"Fixture{k} frame{f} flip{flip}: native RGBA + exact input (bad={bad},hit={hits})");
                if(k==3&&f==10&&!flip)actual.SavePng(Path.Combine(_output,"zombie-hole.png"));
            }
            if(k==0)
            {
                w.ZombieArt!.Publish(icon,10,false);await Frames(2);using var baseline=w.GetTexture().GetImage();
                var night=new CanvasModulate{Color=new(.12f,.18f,.3f)};w.AddChild(night);
                using var white=Image.CreateEmpty(64,64,false,Image.Format.Rgba8);white.Fill(Colors.White);using var lightTexture=ImageTexture.CreateFromImage(white);
                var light=new PointLight2D{Texture=lightTexture,Position=new(32,32),Energy=3,Color=new(1,.4f,.1f)};w.AddChild(light);await Frames(3);
                using var lit=w.GetTexture().GetImage();Check(lit.GetData().SequenceEqual(baseline.GetData()),"Feet and runtime icon retain authored colors under night and warm light");
                light.QueueFree();night.QueueFree();await Frames(2);
            }
            Check(WindowsDesktop.GetBounds(w).End.Y==(int)item.Foot.Y,"Zombie sole stays at physics foot fixture"+k);
            s.Collect(id);c.Publish();for(int i=0;i<19;i++)c.Advance(.02);
        }
        using(var art=new PlagueArt())
        {
            var w=new PlagueEffectWindow(art);AddChild(w);using var source=Image.LoadFromFile(ProjectSettings.GlobalizePath(PlaguePresentation.EffectPath+"Scp049Mutation.png"));
            for(int f=0;f<16;f++)
            {
                var a=new PlagueAura(100,screen.Usable.Position+new Vector2(600,400),f<8?"treat":"transform",f<8?f*.12+.001:(f-8)*.1+.001,false);w.Publish(a,screen.Usable,true);await Frames(2);using var image=w.GetTexture().GetImage();int bad=0;
                for(int y=0;y<96;y++)for(int x=0;x<96;x++){var src=source.GetPixel(f%4*48+x/2,f/4*48+y/2);if(!Near(image.GetPixel(x,y),Over(src,new(0,0,0,0))))bad++;}
                Check(bad==0,$"Aura native frame{f} premultiplied RGBA/nearest (bad={bad})");if(f==10)image.SavePng(Path.Combine(_output,"transform.png"));
            }
            var night=new CanvasModulate{Color=new(.12f,.18f,.3f)};w.AddChild(night);using var white=Image.CreateEmpty(96,96,false,Image.Format.Rgba8);white.Fill(Colors.White);using var texture=ImageTexture.CreateFromImage(white);
            var light=new PointLight2D{Texture=texture,Position=new(48,48),Energy=3,Color=new(1,.4f,.1f)};w.AddChild(light);
            var sample=new PlagueAura(100,screen.Usable.Position+new Vector2(600,400),"transform",.201,false);w.Publish(sample,screen.Usable,true);await Frames(3);using var lit=w.GetTexture().GetImage();night.Color=Colors.White;light.Enabled=false;await Frames(3);using var neutral=w.GetTexture().GetImage();Check(lit.GetData().SequenceEqual(neutral.GetData()),"Aura unshaded color/alpha unchanged by night and warm light");
            foreach(var foot in new[]{screen.Usable.Position+new Vector2I(24,60),new Vector2I(screen.Usable.End.X-24,screen.Usable.Position.Y+60),new Vector2I(screen.Usable.Position.X+24,screen.Usable.End.Y),screen.Usable.End-new Vector2I(24,0)})
            {w.Publish(sample with{Foot=foot},screen.Usable,true);await Frames(2);using var image=w.GetTexture().GetImage();int outside=0;for(int y=0;y<96;y++)for(int x=0;x<96;x++)if(!w.LocalClip.HasPoint(new(x,y))&&image.GetPixel(x,y).A>0)outside++;Check(outside==0&&WindowsDesktop.GetBounds(w).Position==foot-new Vector2I(48,80),"Work-area mask clips effect without shifting target at "+foot);}
            w.Hide();w.QueueFree();await Frames(3);
        }
        s.ResetDoctor();var fixture=Fixture(3);s.ChooseAppearance=()=>fixture.Id;int target=Transform(s,c.Presentation);c.Publish();
        double age0=s.Objects[target].ZombieAnimation!.Age;int auraFrame=c.AuraWindows[target].DisplayedFrame;for(int i=0;i<5;i++)c.Publish();Check(s.Objects[target].ZombieAnimation!.Age==age0&&c.AuraWindows[target].DisplayedFrame==auraFrame,"Repeated Publish does not advance either animation clock");
        c.SetVisible(false);c.Advance(.1);await Frames(2);Check(c.ObjectWindows.Values.All(w=>!w.Visible)&&c.AuraWindows.Values.All(w=>!w.Visible)&&s.Objects[target].ZombieAnimation!.Age==age0,"Hidden game and all native presentation freeze");c.SetVisible(true);
        if(input)await Input(target);
        if(s.Objects.ContainsKey(target))s.Collect(target);c.Publish();
        var cached=_icons.Find(fixture.Id);_catalog=Array.Empty<DesktopIconImage>();_icons._Process(31);_icons._Process(0);c.Publish();
        Check(_icons.Find(fixture.Id)==cached&&c.FadingWindows.Count==1,"Catalogue refresh keeps removed icon texture while fading");
        c.SetVisible(false);for(int i=0;i<10;i++)c.Advance(.1);Check(c.FadingWindows.Values.All(w=>!w.Visible)&&c.Presentation.Disappearing.Count==1,"Hidden disappearance keeps snapshot without accepting input");c.SetVisible(true);
        for(int i=0;i<18;i++)c.Advance(.02);await Frames(3);Check(c.FadingWindows.Count==0&&_icons.CachedCount==0,"Fade completion releases window and last uncatalogued icon reference");
        _catalog=Enumerable.Range(0,6).Select(Fixture).ToArray();_icons._Process(31);_icons._Process(0);s.ChooseAppearance=()=>Fixture(0).Id;
        var ids=new List<int>();for(int i=0;i<8;i++){s.ResetDoctor();int id=Transform(s,c.Presentation);ids.Add(id);s.Objects[id].Physics.Place(screen.Usable.Position+new Vector2(650+i*28,500));c.Publish();}
        var process=Process.GetCurrentProcess();var timer=Stopwatch.StartNew();double cpu=process.TotalProcessorTime.TotalMilliseconds;await Frames(90);process.Refresh();double withEffects=process.TotalProcessorTime.TotalMilliseconds-cpu,withEffectsSeconds=timer.Elapsed.TotalSeconds;int windows=c.ObjectWindows.Count+c.AuraWindows.Count;
        for(int i=0;i<45;i++)c.Advance(.02);cpu=process.TotalProcessorTime.TotalMilliseconds;timer.Restart();await Frames(90);process.Refresh();_cost=new{objects=8,windowsWithEffects=windows,withEffectsSeconds,cpuWithEffectsMs=withEffects,withoutEffectsSeconds=timer.Elapsed.TotalSeconds,cpuWithoutEffectsMs=process.TotalProcessorTime.TotalMilliseconds-cpu,workingSet=process.WorkingSet64,gpu="not measured",longRun="not measured"};
        Check(c.ObjectWindows.Count==8&&c.AuraWindows.Count==0,"Eight overlapping zombies remain after effects finish");
        foreach(int id in ids)s.Collect(id);c.Publish();Check(c.FadingWindows.Count==8,"Eight independent decorative disappearances coexist");c.Shutdown();_objects.Shutdown();await Frames(3);Check(c.ObjectWindows.Count==0&&c.FadingWindows.Count==0&&c.AuraWindows.Count==0&&s.Stopped,"Shutdown immediately frees every actor/aura/fade window");
    }
    private async Task Input(int id)
    {
        var c=_controller!;var s=c.State;var item=s.Objects[id];var area=s.Screens[0].Usable;item.Physics.Place(area.Position+new Vector2(800,420));c.Publish();
        using var receiver=new NativeInputFixture(_output);for(int i=0;i<300&&!receiver.Ready;i++)await Frames(1);Check(receiver.Ready,"Owned receiver ready");
        bool focus=false;for(int i=0;i<60&&!focus;i++){focus=receiver.Focus();await Frames(1);}Check(focus,"Owned receiver has focus before input");
        var w=c.ObjectWindows[id];Vector2 top=WindowsDesktop.GetBounds(w).Position;
        foreach(var point in new[]{new Vector2(1,31),new Vector2(32,32),new Vector2(63,30)})
        {receiver.Move(top+point);c.Publish();await Frames(4);var before=receiver.Counts();receiver.Click();await Frames(4);var after=receiver.Counts();Check(after.Down==before.Down+1&&after.Up==before.Up+1,"Actual transparent hole/old hand position passes click "+point);}
        var aura=c.AuraWindows[id];Vector2 auraPoint=WindowsDesktop.GetBounds(aura).Position+new Vector2(48,6);receiver.Move(auraPoint);c.Publish();await Frames(4);var b=receiver.Counts();receiver.Click();await Frames(4);var e=receiver.Counts();Check(e.Down==b.Down+1&&e.Up==b.Up+1,"Actual aura window forwards click across processes");
        Vector2 opaque=new(20,20);Check(w.Hit(opaque),"Fixture opaque point for actual drag");receiver.Move(top+opaque);c.Publish();await Frames(4);Vector2 start=item.Foot;int batches=_stack!.AppliedBatches;
        // Keep the same automatic input path as the product; test does not synthesize state drag calls.
        c.Automatic=true;receiver.Press(true);await Frames(5);receiver.Move(top+opaque+new Vector2(100,-70));await Frames(5);Check(s.DraggedObject==id&&item.Foot.DistanceTo(start+new Vector2(100,-70))<3,"Real right drag follows alpha body and reaches expected position");
        receiver.Release();await Frames(3);c.Automatic=false;Check(s.DraggedObject==null&&_stack.AppliedBatches-batches<=6,"Release ends drag with bounded common stack changes");
        top=WindowsDesktop.GetBounds(w).Position;receiver.Move(top+opaque);c.Publish();await Frames(4);int count=s.Collected;receiver.Click();await Frames(4);Check(!s.Objects.ContainsKey(id)&&s.Collected==count+1&&c.FadingWindows.ContainsKey(id),"Real left click collects once and creates decorative fade");
        b=receiver.Counts();receiver.Click();await Frames(4);e=receiver.Counts();Check(e.Down==b.Down+1&&e.Up==b.Up+1&&s.Collected==count+1,"Click through visible fade reaches external receiver without double reward");
    }
}
