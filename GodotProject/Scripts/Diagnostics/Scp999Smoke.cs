using Godot;
using SiteDesktop.App;
using SiteDesktop.Companions;
using SiteDesktop.Desktop;
using SiteDesktop.Overlay;
using SiteDesktop.Simulation;
using SiteDesktop.Platforms;
using SiteDesktop.Platform.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;

public partial class Scp999Smoke : Node
{
    private readonly List<string> _checks=new();
    private string _output="";
    private Main? _main;
    private bool _native;
    private bool _nativeInput;
    private void Require(bool value,string name)
    {if(!value)throw new InvalidOperationException(name);_checks.Add(name);GD.Print("SCP999_CHECK_PASS "+name);}
    private static Scp999State Create(Scp999Settings? settings=null)
    {
        var screens=new[]{new DesktopScreen(0,new Rect2I(-1280,0,1280,800),new Rect2I(-1280,0,1280,760),96),new DesktopScreen(1,new Rect2I(0,0,1920,1080),new Rect2I(0,0,1920,1040),144)};
        return new(screens,new SpriteAnimationSet(Scp999Window.AssetPath+"Scp999.contract.json"),new SpriteAnimationSet("res://Assets/Sprites/vfx/Scp999Reaction/Scp999Reaction.contract.json"),new SpriteAnimationSet("res://Assets/Sprites/vfx/Scp999Trail/Scp999Trail.contract.json"),new StressState(),settings);
    }
    private static void Step(Scp999State s,double seconds,Vector2 pointer,bool left=false,bool right=false,bool over=false)
    {int count=(int)Math.Ceiling(seconds/.02);for(int i=0;i<count;i++)s.Tick(Math.Min(.02,seconds-i*.02),pointer,left,right,over);}
    private void Logic()
    {
        var s=Create();var p=s.Position;
        Step(s,.5,s.Foot+new Vector2(220,0));
        Require(Math.Abs(s.Position.X-p.X-80)<.01 && s.Tag=="move","follow speed, negative desktop origin and move tag");
        Require(s.Position.Y==p.Y,"movement has no baked vertical bounce");
        Step(s,.1,s.Foot+new Vector2(10,0));Require(s.Tag=="idle","follow dead zone");
        p=s.Position;Step(s,.2,s.Foot+new Vector2(700,0));Require(s.Position==p,"out-of-range cursor ignored");
        Step(s,.2,s.Foot+new Vector2(-160,0));Require(s.Flipped&&s.Tag=="move","direction reversal");
        var stress=s.Stress;stress.Add(new("inside",s.Foot,50));stress.Add(new("edge",s.Foot+new Vector2(240,0),50));stress.Add(new("outside",s.Foot+new Vector2(241,0),50));
        Require(s.BeginPet(s.Foot-new Vector2(0,30)),"pet begins through common command");
        Require(!s.BeginPet(s.Foot),"duplicate press ignored");
        Require(stress.Targets["inside"].Value==47&&stress.Targets["edge"].Value==47&&stress.Targets["outside"].Value==50,"stress inclusive radius and outside exclusion");
        int typing=s.TypingReactions;s.TypingActivity();Require(s.TypingReactions==typing,"pet priority over typing");
        var pointer=s.Foot-new Vector2(0,30);Step(s,.2,pointer,true,false,true);float stationary=stress.Targets["inside"].Value;
        Step(s,.5,pointer,true,false,true);Require(stress.Targets["inside"].Value==stationary,"stationary hold does not repeatedly count as rubbing");
        for(int i=0;i<10;i++)s.Tick(.02,pointer+new Vector2(i%2==0?8:-8,0),true,false,true);
        Require(stress.Targets["inside"].Value<stationary&&stress.Targets["outside"].Value==50,"rubbing relieves shared state within range");
        s.Tick(.02,pointer,false);Require(s.Tag=="pet_recover"&&s.Gesture==CompanionGesture.None,"release outside recovers pet");
        Step(s,1,s.Foot);s.TypingActivity();int reactions=s.TypingReactions;for(int i=0;i<100;i++)s.TypingActivity();
        Require(s.Tag=="typing_react"&&s.TypingReactions==reactions&&s.Effects.Count(e=>e.Kind==CompanionEffectKind.Reaction)==1,"typing burst merged and reaction spawned");
        Step(s,1.5,s.Foot);Require(!s.Effects.Any(e=>e.Kind==CompanionEffectKind.Reaction)&&s.Tag=="idle","reaction lifetime and one-shot return");
        var dragStart=s.Position;s.BeginDrag(s.Position+new Vector2(20,20));s.TypingActivity();s.Tick(.02,new Vector2(150,250),false,true);
        Require(s.Position==new Vector2(130,230)&&s.Gesture==CompanionGesture.Drag,"right drag across physical monitors without extra DPI scale");
        s.Tick(.02,new Vector2(150,250),false,false);Require(s.Tag=="fall"&&s.Gesture==CompanionGesture.None,"drag release starts actual gravity");
        Step(s,1,s.Foot);s.ResetPosition();
        for(int i=0;i<30;i++)s.Tick(.02,s.Foot+new Vector2(200,0));
        Require(s.Effects.Any(e=>e.Kind==CompanionEffectKind.Trail),"ground movement emits trail");
        var trail=s.Effects.First(e=>e.Kind==CompanionEffectKind.Trail);var origin=trail.Origin;
        Step(s,.1,s.Foot+new Vector2(200,0));Require(trail.Origin==origin,"trail does not follow body after spawn");
        s.Physics.Place(s.Foot-new Vector2(0,200));int countTrails=s.Effects.Count;Step(s,.1,s.Foot+new Vector2(200,0));
        Require(s.Tag=="fall"&&s.Effects.Count<=countTrails,"actual airborne state prevents new trail");
        Step(s,1.5,s.Foot);Require(s.Physics.Body.SupportId!=null&&s.Tag=="idle","actual landing recovers to idle");
        PlatformLogic();
        var preserved=s.Position;float before=stress.Targets["inside"].Value;s.SetVisible(false);s.TypingActivity();s.BeginPet(s.Foot);Step(s,5,new Vector2(900,900),true,true,true);
        Require(s.Position==preserved&&s.Effects.Count==0&&stress.Targets["inside"].Value==before,"hide cancels gestures/VFX and freezes common state");
        s.SetVisible(true);Require(s.Position==preserved&&s.Tag=="idle","show preserves position without queued actions");
        s.ReplaceScreens(new[]{s.Screens[1]});Require(s.Position.X>=0&&s.Gesture==CompanionGesture.None,"removed monitor recovery");
        stress.Targets["inside"].Set(-4);Require(stress.Targets["inside"].Value==0,"stress clamps lower bound");stress.Targets["inside"].Set(120);Require(stress.Targets["inside"].Value==100,"stress clamps upper bound");
        var limited=Create(new Scp999Settings{MaxTrails=2,TrailSpacing=1,TrailInterval=.02,MaxReactions=2,ReactionCooldown=.02});
        for(int i=0;i<100;i++){limited.Tick(.02,limited.Foot+new Vector2(200,0));}
        Require(limited.Effects.Count(e=>e.Kind==CompanionEffectKind.Trail)==2,"trail cap removes oldest");
        for(int i=0;i<30;i++){limited.Tick(.02,limited.Foot);limited.TypingActivity();}
        Require(limited.Effects.Count(e=>e.Kind==CompanionEffectKind.Reaction)==2,"reaction cap under sustained input");
        limited.SetVisible(false);Require(limited.Effects.Count==0,"all effect categories cleared together");
        limited.Shutdown();limited.SetVisible(true);limited.TypingActivity();Require(limited.Stopped&&!limited.Visible&&limited.Effects.Count==0,"shutdown is terminal and idempotent");limited.Shutdown();
        var hiddenDrag=Create();float floor=hiddenDrag.Position.Y;hiddenDrag.BeginDrag(hiddenDrag.Position);hiddenDrag.Tick(.02,hiddenDrag.Position-new Vector2(0,200),false,true);hiddenDrag.SetVisible(false);hiddenDrag.SetVisible(true);
        Require(hiddenDrag.Position.Y==floor-200&&hiddenDrag.Gesture==CompanionGesture.None,"hide cancels drag while preserving airborne position");
        foreach(var clip in s.Body.Clips.Values)
        {
            Require(s.Body.FrameAt(clip.Name,0)==clip.From,$"{clip.Name} starts at correct frame");
            Require(s.Body.FrameAt(clip.Name,s.Body.Duration(clip.Name)+.001)==(clip.Loop?clip.From:clip.To),$"{clip.Name} loop/one-shot contract");
        }
        Require(s.Trails.FrameAt("puddle_a",1.5,1.8)==2&&s.Trails.FrameAt("puddle_a",2.7,1.8)==8,"trail hold extension and transparent end");
        Require(s.Reactions.FrameAt("note",.85)==15,"reaction transparent end frame");
    }
    private void PlatformLogic()
    {
        var s=Create();var a=new WindowObservation("A",new(-1100,500,500,200));var b=new WindowObservation("B",new(-500,400,450,200));
        s.ObserveWindows(new[]{b,a});s.Physics.Place(new(-850,360));Step(s,.4,new(-850,500));
        Require(s.Tag=="land"&&s.Physics.Body.SupportId=="A","actual collision starts land animation");
        Step(s,.6,s.Foot);for(int i=0;i<30;i++)s.Tick(.02,s.Foot+new Vector2(200,0));
        var trail=s.Effects.First(e=>e.Kind==CompanionEffectKind.Trail);var origin=trail.Origin;var foot=s.Foot;
        a=a with{Bounds=new(-1060,480,500,200)};s.ObserveWindows(new[]{b,a});
        Require(s.Tag=="balance"&&s.Foot==foot+new Vector2(40,-20)&&trail.Origin==origin+new Vector2(40,-20),"moving ledge carries body and attached trail");
        int reactions=s.TypingReactions;s.TypingActivity();Require(!s.BeginPet(s.Foot)&&s.TypingReactions==reactions,"balance has priority over pet and typing");
        Step(s,.28,s.Foot);Require(s.Tag=="balance_recover","balance recovery animation retained");Step(s,1,s.Foot);
        s.Physics.Place(new(a.Bounds.End.X-90,a.Bounds.Position.Y-100));Step(s,.8,new(a.Bounds.End.X-90,a.Bounds.Position.Y));
        Require(s.Physics.JumpToAdjacent(1),"companion footprint fits adjacent jump");s.Tick(0,s.Foot);Require(s.Tag=="takeoff","shared jump emits takeoff tag");
        Step(s,1.2,s.Foot);Require(s.Physics.Body.SupportId=="B","companion lands on adjacent ledge");
        var auto=Create();auto.ObserveWindows(new[]{b,a});auto.Physics.Place(new(a.Bounds.End.X-90,a.Bounds.Position.Y-100));Step(auto,.8,new(a.Bounds.End.X-90,a.Bounds.Position.Y));
        for(int i=0;i<90;i++)auto.Tick(.02,auto.Foot+new Vector2(200,0));
        Require(auto.Physics.Body.Jumps==1&&auto.Physics.Body.SupportId=="B","cursor following automatically jumps using companion footprint");
        var pet=Create();pet.ObserveWindows(new[]{a});pet.Physics.Place(new(a.Bounds.Position.X+220,a.Bounds.Position.Y-100));Step(pet,1.2,new(a.Bounds.Position.X+220,a.Bounds.Position.Y));
        Require(pet.BeginPet(pet.Foot),"pet can start on actual window ledge");pet.ObserveWindows(Array.Empty<WindowObservation>());
        Require(pet.Gesture==CompanionGesture.None&&pet.Tag=="fall","lost support interrupts held pet before further stress relief");
        s.ObserveWindows(new[]{b,a with{Minimized=true}});Require(!s.Effects.Any(e=>e.SupportId=="A"),"minimize removes attached trail");
        s.ObserveWindows(Array.Empty<WindowObservation>());Require(s.Tag=="fall","removed ledge emits fall");
        Step(s,2,s.Foot);Require(s.Physics.Body.SupportId=="floor:0","companion settles to floor after ledge removal");
    }
    private async Task Frames(int count){for(int i=0;i<count;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private async Task NativeInput(Scp999Controller controller)
    {
        GD.Print($"INPUT_ROOT handle={DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,GetWindow().GetWindowId())} bounds={WindowsDesktop.GetBounds(GetWindow())}");
        WindowsDesktop.SetControlVisible(GetWindow(),false);
        controller.AutomaticInput=true;controller.SetVisible(true);controller.AutomaticInput=false;controller.ResetPositions();
        using var fixture=new NativeInputFixture(_output);
        async Task Click(){fixture.Press();await Frames(3);fixture.Release();await Frames(6);}
        for(int i=0;i<180&&!fixture.Ready;i++)await Frames(1);
        Require(fixture.Ready,"separate owned input receiver ready");
        await Frames(30);bool focused=false;
        for(int i=0;i<30&&!focused;i++){focused=fixture.Focus();await Frames(1);}
        Require(focused,"owned receiver acquired foreground before synthetic input");fixture.PlaceBehind(controller.BodyWindow!);controller.RefreshPlatforms();await Frames(6);
        var s=controller.State;var point=s.Position+new Vector2(64,90);
        fixture.Move(point);await Frames(3);var before=fixture.Counts();int presses=s.PetPresses;
        GD.Print($"BODY_INPUT_TRACE actual={NativeInputFixture.WindowAt(point)} expected={DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,controller.BodyWindow!.GetWindowId())} point={point} bounds={WindowsDesktop.GetBounds(controller.BodyWindow)} hit={controller.BodyWindow.ContainsDesktop(point)} motion={s.Motion} tag={s.Tag}");
        controller.Stress.Add(new("native-neighbor",s.Foot,50));
        await Click();
        Require(s.PetPresses==presses+1&&fixture.Counts().Down==before.Down&&controller.Stress.Targets["native-neighbor"].Value==47,"native body click reaches pet/shared stress, not receiver");
        s.EndGesture();for(int i=0;i<50;i++)controller.Advance(.02,s.Foot);
        fixture.Move(s.Position+new Vector2(4,4));await Frames(3);before=fixture.Counts();await Click();
        GD.Print($"CORNER_TRACE point={s.Position+new Vector2(4,4)} window={NativeInputFixture.WindowAt(s.Position+new Vector2(4,4))} receiver={fixture.Handle} actual={(WindowsDesktop.TryGetPointer(out var actual,out _)?actual:Vector2.Zero)} counts={fixture.Counts()} before={before} hit={controller.BodyWindow!.ContainsDesktop(s.Position+new Vector2(4,4))}");
        Require(fixture.Counts().Down==before.Down+1&&fixture.Counts().Up==before.Up+1,"native transparent corner passes down and up to separate process");
        fixture.Move(s.Position+new Vector2(64,90));await Frames(3);int typing=s.TypingReactions;before=fixture.Counts();
        controller.AutomaticInput=true;fixture.KeyActivity();await Frames(6);controller.AutomaticInput=false;
        Require(s.TypingReactions==typing+1&&fixture.Counts().Keys==before.Keys+1,"global activity hook reacts while receiver retains keyboard focus");
        var reaction=controller.EffectWindows.First(w=>w.Effect.Kind==CompanionEffectKind.Reaction);
        using(var capture=DisplayServer.ScreenGetImageRect(new Rect2I(WindowsDesktop.ToGodotPosition((Vector2I)s.Position+new Vector2I(-40,-60)),new Vector2I(208,180))))capture.SavePng(Path.Combine(_output,"reaction-desktop.png"));
        Require(!controller.BodyWindow!.ContainsDesktop(reaction.Effect.Position),"reaction input probe lies outside body silhouette");
        fixture.Move(reaction.Effect.Position);await Frames(3);before=fixture.Counts();await Click();
        Require(fixture.Counts().Down==before.Down+1&&fixture.Counts().Up==before.Up+1,"native reaction window passes down and up to separate process");
        for(int i=0;i<60;i++)controller.Advance(.02,s.Foot);
        for(int i=0;i<70;i++)controller.Advance(.02,s.Foot+new Vector2(200,0));await Frames(3);
        var trail=controller.EffectWindows.First(w=>w.Effect.Kind==CompanionEffectKind.Trail);
        using(var capture=DisplayServer.ScreenGetImageRect(new Rect2I(WindowsDesktop.ToGodotPosition((Vector2I)s.Position+new Vector2I(-240,-20)),new Vector2I(368,140))))capture.SavePng(Path.Combine(_output,"trail-desktop.png"));
        fixture.Move(trail.Effect.Position);await Frames(3);before=fixture.Counts();await Click();
        Require(fixture.Counts().Down==before.Down+1&&fixture.Counts().Up==before.Up+1,"native trail window passes down and up to separate process");
        controller.ResetPositions();point=s.Position+new Vector2(64,90);fixture.Move(point);await Frames(3);
        controller.Stress.Targets["native-neighbor"].Position=s.Foot;
        float stressBefore=controller.Stress.Targets["native-neighbor"].Value;
        controller.AutomaticInput=true;fixture.Press();await Frames(3);
        for(int i=0;i<8;i++){fixture.Move(point+new Vector2(i%2==0?8:-8,0));await Frames(3);}
        fixture.Release();await Frames(3);controller.AutomaticInput=false;
        Require(controller.Stress.Targets["native-neighbor"].Value<stressBefore-s.Settings.ClickRelief&&s.Gesture==CompanionGesture.None,"native held rubbing reduces common stress and release ends gesture");
        for(int i=0;i<50;i++)controller.Advance(.02,s.Foot);
        point=s.Position+new Vector2(64,90);fixture.Move(point);await Frames(3);var start=s.Position;
        controller.AutomaticInput=true;fixture.Press(true);await Frames(3);fixture.Move(point+new Vector2(120,-120));await Frames(6);
        Require(s.Gesture==CompanionGesture.Drag&&s.Position.DistanceTo(start+new Vector2(120,-120))<2,"native right drag uses global pointer polling");
        fixture.Release();await Frames(3);controller.AutomaticInput=false;
        Require(s.Gesture==CompanionGesture.None&&s.Tag=="fall"&&s.Position.Y<start.Y,"native right release starts falling");
        for(int i=0;i<50;i++)controller.Advance(.02,s.Foot);
        start=s.Position;fixture.Move(s.Foot+new Vector2(200,-20));controller.AutomaticInput=true;await Frames(20);controller.AutomaticInput=false;
        Require(s.Position.X>start.X&&s.Tag=="move","native cursor drives follow movement");
        fixture.Move(s.Foot);s.TypingActivity();controller.Publish();
        Require(s.Effects.Any(e=>e.Kind==CompanionEffectKind.Reaction),"fresh effect present before real-time expiry check");
        controller.AutomaticInput=true;await Frames(65);controller.AutomaticInput=false;
        Require(s.Tag=="idle"&&!s.Effects.Any(e=>e.Kind==CompanionEffectKind.Reaction),"real elapsed frames finish reaction and settle to idle");
        uint bodyDpi=WindowsDesktop.GetDpi(controller.BodyWindow!);
        int beforeHiddenTyping=s.TypingReactions;
        point=s.Position+new Vector2(64,90);controller.SetVisible(false);await Frames(3);
        fixture.Move(point);await Frames(3);before=fixture.Counts();await Click();fixture.KeyActivity();await Frames(6);
        Require(fixture.Counts().Down==before.Down+1&&fixture.Counts().Keys==before.Keys+1&&s.TypingReactions==beforeHiddenTyping,"hidden body passes input and generates no typing reaction");
        File.WriteAllText(Path.Combine(_output,"desktop.json"),JsonSerializer.Serialize(new{screens=s.Screens.Select(x=>new{index=x.Index,bounds=x.Bounds.ToString(),usable=x.Usable.ToString(),dpi=x.Dpi}),bodyDpi,receiver=fixture.Counts().ToString()},new JsonSerializerOptions{WriteIndented=true}));
        controller.AutomaticInput=true;controller.SetVisible(true);controller.AutomaticInput=false;
    }
    public override async void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();for(int i=0;i<args.Length;i++)if(args[i]=="--output")_output=args[++i];
            _nativeInput=args.Contains("--native-input");
            if(_output==""||File.Exists(Path.Combine(_output,"results.json")))throw new InvalidOperationException("A new output directory is required.");Directory.CreateDirectory(_output);
            Logic();
            _main=GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();_main.PersistScpSettings=false;_main.EnableSystemOptions=false;_main.EnableStatue=false;_main.EnablePlague=false;AddChild(_main);
            var controller=_main.Companion;controller.AutomaticInput=false;
            await Frames(5);_native=DisplayServer.GetName()!="headless";
            if(_native)
            {
                var window=controller.BodyWindow!;
                Require(window.Visible&&!window.IsEmbedded()&&window.Transparent&&window.TransparentBg&&window.Unfocusable,"native body window flags");
                Require(WindowsDesktop.GetBounds(window).Position==controller.State.Position.Round(),"native physical origin matches state");
                Require(window.MousePassthroughPolygon.Length>8,"art silhouette input region replaces placeholder");
                Require(controller.KeyboardMonitoring,"Windows keyboard activity subscription installed");
                controller.State.TypingActivity();controller.Publish();await Frames(3);
                Require(controller.EffectWindows.Count==1&&controller.EffectWindows.All(w=>w.MousePassthrough&&w.Unfocusable&&w.Visible),"native reaction fully click-through");
                for(int i=0;i<70;i++)controller.Advance(.02,controller.State.Foot);
                for(int i=0;i<60;i++)controller.Advance(.02,controller.State.Foot+new Vector2(200,0));
                await Frames(3);Require(controller.EffectWindows.Any(w=>w.Effect.Kind==CompanionEffectKind.Trail)&&controller.EffectWindows.All(w=>w.MousePassthrough),"native trail independent windows");
                using(var capture=window.GetTexture().GetImage())capture.SavePng(Path.Combine(_output,"body.png"));
                foreach(var w in controller.EffectWindows.Take(1)){using var capture=w.GetTexture().GetImage();capture.SavePng(Path.Combine(_output,"trail.png"));}
                controller.SetVisible(false);await Frames(3);
                Require(!window.Visible&&!controller.KeyboardMonitoring&&controller.EffectWindows.Count==0,"hide releases Windows input subscription and all effect windows");
                controller.AutomaticInput=true;controller.SetVisible(true);controller.AutomaticInput=false;await Frames(3);
                Require(window.Visible&&controller.KeyboardMonitoring,"restore reinstalls input without stale VFX");
                await TrailRendering(controller);
                await NativePlatforms(controller);
                if(_nativeInput)await NativeInput(controller);
                controller.State.ResetPosition();controller.State.TypingActivity();controller.Publish();
                Require(controller.EffectWindows.Any(w=>w.Effect.Kind==CompanionEffectKind.Reaction),"fresh reaction before wall-clock expiry");
                var timer=System.Diagnostics.Stopwatch.StartNew();double last=0;
                while(timer.Elapsed.TotalSeconds<1.1)
                {await Frames(1);double now=timer.Elapsed.TotalSeconds;controller.Advance(now-last,controller.State.Foot);last=now;}
                Require(!controller.State.Effects.Any(e=>e.Kind==CompanionEffectKind.Reaction)&&controller.EffectWindows.Count==0,"wall-clock reaction expiry removes native window");
            }
            controller.Shutdown();await Frames(3);
            Require(controller.State.Stopped&&!controller.KeyboardMonitoring&&controller.EffectWindows.Count==0&&controller.BodyWindow==null,"product shutdown releases resources");
            File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new{passed=true,checks=_checks.Count,native=_native,nativeInput=_nativeInput,tests=_checks,limitations=new[]{"DWM rectangular window bounds; actual mixed-DPI hardware not tested",_nativeInput?"Input delivery checked only against owned separate process":"No synthetic OS input in this run; see run-021 for delivery evidence","Long-duration performance and mixed-DPI hardware not measured in this run"}},new JsonSerializerOptions{WriteIndented=true}));
            GD.Print($"SCP999_SMOKE_PASS checks={_checks.Count} native={_native}");_main.RequestExit();
        }
        catch(Exception ex){GD.PushError("SCP999_SMOKE_FAILED "+ex);_main?.Companion.Shutdown();GetTree().Quit(1);}
    }
}
