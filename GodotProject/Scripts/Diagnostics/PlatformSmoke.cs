using Godot;
using SiteDesktop.App;
using SiteDesktop.Desktop;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Platforms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;

public partial class PlatformSmoke : Node
{
    private readonly List<string> _checks = new();
    private readonly List<Window> _owned = new();
    private string _output = "";
    private PlatformDesktop? _app;
    private readonly List<object> _nativeTrace = new();
    private void Require(bool ok, string label)
    { if (!ok) throw new InvalidOperationException(label); _checks.Add(label); GD.Print("PLATFORM_CHECK_PASS " + label); }
    private static DesktopScreen Screen => new(0, new(-1920, 1061, 1920, 1080), new(-1920, 1061, 1920, 1032), 144);
    private static WindowObservation A => new("A", new(-1700, 1661, 500, 300));
    private static WindowObservation B => new("B", new(-1100, 1561, 450, 300));
    private static void Step(PlatformWorld w, double time)
    { int frames = (int)Math.Ceiling(time * 120); for (int i = 0; i < frames; i++) w.Tick(Math.Min(1d / 120, time - i / 120d)); }
    private PlatformWorld Standing()
    {
        var w = new PlatformWorld(new[] { Screen }); w.Observe(new[] { B, A });
        w.Place(new(-1450, 1520)); Step(w, .6);
        Require(w.Body.SupportId == "A" && w.Body.Foot.Y == 1661, "downward sweep lands on exposed top at negative desktop origin");
        return w;
    }
    private void Logic()
    {
        var w = Standing(); float x = w.Body.Foot.X; w.Walk(1); Step(w, .25);
        Require(Math.Abs(w.Body.Foot.X - x - 40) < .02 && w.Body.SupportId == "A", "walking uses physical pixels without a second DPI multiplier");
        w.Walk(0);
        w.Observe(new[] { B, A with { Bounds = new(-1660, 1641, 500, 300) } });
        Require(Math.Abs(w.Body.Foot.X - x - 80) < .02 && w.Body.Foot.Y == 1641 && w.Body.Motion == PlatformMotion.Balancing, "moving support carries body and emits balancing state");
        float carried = w.Body.Foot.X; Step(w, .12);
        Require(w.Body.Foot.X < carried, "support movement generates bounded inertial slip");
        w.Observe(new[] { B, A with { Minimized = true } });
        Require(w.Body.SupportId == null && w.Body.Motion == PlatformMotion.Falling, "minimized support immediately releases body");
        Step(w, 2); Require(w.Body.SupportId == "floor:0" && w.Body.Foot.Y == Screen.Usable.End.Y - 8, "missing support falls to work-area floor above taskbar");
        w = Standing(); w.Observe(Array.Empty<WindowObservation>()); Step(w, .1);
        Require(w.Body.SupportId == null && w.Body.Foot.Y > 1661, "closed support falls through same update path");
        w = Standing(); w.Observe(new[] { A with { Bounds = new(-1300, 1661, 500, 300) } });
        Require(w.Body.SupportId == null, "large support teleport releases instead of flinging body");
        var cover = new WindowObservation("cover", new(-1500, 1500, 200, 250));
        w = new(new[] { Screen }); w.Observe(new[] { cover, A });
        var pieces = w.Surfaces.Where(s => s.Id == "A").ToArray();
        Require(pieces.Length == 2 && pieces[0].Right == -1500 && pieces[1].Left == -1300, "front window cuts covered platform into exposed segments");
        w.Observe(new[] { A, cover }); Require(w.Surfaces.Single(s => s.Id == "A").Right == -1200, "z-order reversal restores front edge");
        w.Observe(new[] { new WindowObservation("max", Screen.Usable), A });
        Require(w.Surfaces.All(s => s.Floor), "maximized top has no headroom and hides lower windows");
        w.Observe(new[] { A with { Visible = false }, B with { Cloaked = true } });
        Require(w.Surfaces.All(s => s.Floor), "hidden and cloaked windows cannot create platforms");
        w.Observe(new[] { A with { CanSupport = false } });
        Require(w.Surfaces.All(s => s.Floor), "non-supporting window has no ledge");
        w.Observe(new[] { A }); w.Place(new(-1450, 1680)); Step(w, .2);
        Require(w.Body.Foot.Y > 1680 && w.Body.SupportId == null, "one-way ledge never pulls a body upward from below");
        w = Standing(); w.Place(new(-1250, 1520)); Step(w, .6); w.Walk(0);
        Require(w.JumpToAdjacent(1) && w.Body.Motion == PlatformMotion.Takeoff, "reachable adjacent ledge starts a ballistic jump");
        Step(w, 1.1); Require(w.Body.SupportId == "B", "ballistic jump lands on neighboring upper ledge");
        w = Standing(); w.Place(new(-1250, 1520)); Step(w, .6); w.Walk(1); Step(w, 1.2);
        Require(w.Body.Jumps == 1 && w.Body.SupportId == "B", "walking toward edge automatically jumps to adjacent reachable platform");
        w = Standing(); w.Observe(new[] { A }); w.Body.AutoJump = false; w.Walk(1); Step(w, 2);
        Require(w.Body.SupportId != "A" && w.Body.Motion == PlatformMotion.Falling, "walking beyond ledge falls when no jump is available");
        w = Standing(); Require(!w.JumpToAdjacent(-1), "no target in requested direction leaves support unchanged");
        var foot = w.Body.Foot; w.SetVisible(false); w.Observe(Array.Empty<WindowObservation>()); Step(w, 10);
        Require(w.Body.Foot == foot && w.Body.SupportId == "A", "hide freezes physics and environment application");
        w.SetVisible(true); w.Observe(Array.Empty<WindowObservation>()); Step(w, .02);
        Require(w.Body.SupportId == null && w.Body.Foot.Y > foot.Y, "show validates latest support without hidden-time catch-up");
        w.BeginDrag(w.Body.Foot); w.Drag(new(-1300, 1400)); w.EndDrag(); Step(w, .1);
        Require(w.Body.Motion == PlatformMotion.Falling && w.Body.Foot.Y > 1400, "drag release uses gravity rather than floor snap");
        w.BeginDrag(w.Body.Foot); w.SetVisible(false); w.SetVisible(true);
        Require(w.Body.Motion != PlatformMotion.Dragging, "hide cancels drag capture");
        w.ReplaceScreens(new[] { new DesktopScreen(1, new(0,0,1280,720), new(0,0,1280,680),96) });
        Require(w.Body.Foot.X >= 24 && w.Body.Foot.X <= 1256, "removed monitor recovers into remaining work area");
        Step(w,2);Require(w.Body.SupportId=="floor:1","monitor recovery cannot strand body below floor collision plane");
        var blocked=Standing();blocked.Observe(new[]{A,new WindowObservation("lower",new(-1700,1761,500,200))});
        Require(!blocked.JumpToAdjacent(),"lower overlapping target blocked by current ledge is not reachable");
        w.Stop(); var stopped = w.Body.Foot; w.SetVisible(true); w.Tick(1);
        Require(w.Stopped && !w.Visible && w.Body.Foot == stopped, "shutdown is terminal and freezes body");
        Replay();
    }
    private void Replay()
    {
        const string input = """
        {"screen":[-1920,1061,1920,1032],"steps":[
          {"command":"observe","rect":[-1700,1661,500,300]},
          {"command":"place","point":[-1450,1500]},
          {"command":"tick","seconds":0.7},
          {"command":"observe","rect":[-1660,1641,500,300]},
          {"command":"tick","seconds":0.2},
          {"command":"hide"},{"command":"tick","seconds":8},
          {"command":"show"},{"command":"remove"},{"command":"tick","seconds":2}]}
        """;
        File.WriteAllText(Path.Combine(_output, "scenario-inputs.json"), input);
        string Run()
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(_output,"scenario-inputs.json")));
            var area = doc.RootElement.GetProperty("screen").EnumerateArray().Select(v => v.GetInt32()).ToArray();
            var w = new PlatformWorld(new[] { new DesktopScreen(0,new(area[0],area[1],area[2],area[3]),new(area[0],area[1],area[2],area[3]),144) });
            var trace = new List<object>();
            foreach (var op in doc.RootElement.GetProperty("steps").EnumerateArray())
            {
                var command = op.GetProperty("command").GetString();
                switch (command)
                {
                    case "observe": var r = op.GetProperty("rect").EnumerateArray().Select(v=>v.GetInt32()).ToArray(); w.Observe(new[]{new WindowObservation("replay",new(r[0],r[1],r[2],r[3]))}); break;
                    case "place": var p = op.GetProperty("point").EnumerateArray().Select(v=>v.GetInt32()).ToArray(); w.Place(new(p[0],p[1])); break;
                    case "tick": Step(w,op.GetProperty("seconds").GetDouble()); break;
                    case "hide": w.SetVisible(false); break;
                    case "show": w.SetVisible(true); break;
                    case "remove": w.Observe(Array.Empty<WindowObservation>()); break;
                }
                trace.Add(new {command,x=w.Body.Foot.X,y=w.Body.Foot.Y,support=w.Body.SupportId,motion=w.Body.Motion.ToString(),w.Visible});
            }
            return JsonSerializer.Serialize(trace,new JsonSerializerOptions{WriteIndented=true});
        }
        string first=Run(), second=Run(); File.WriteAllText(Path.Combine(_output,"replay-trace.json"),first);
        Require(first==second,"recorded environment and elapsed-time input replay produces identical trace");
    }
    private async Task Frames(int count) { for(int i=0;i<count;i++) await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame); }
    private static nint Handle(Window w)=>(nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,w.GetWindowId());
    private Window Fixture(string title,Rect2I bounds,bool backdrop=false)
    {
        var w=new Window{Visible=false,Title=title,ForceNative=true,Transient=false,AlwaysOnTop=true,Borderless=backdrop,
            Unresizable=backdrop,InitialPosition=Window.WindowInitialPosition.Absolute,ContentScaleMode=Window.ContentScaleModeEnum.Disabled,
            Position=WindowsDesktop.ToGodotPosition(bounds.Position),Size=bounds.Size};
        var panel=new ColorRect{Color=backdrop?new Color("26323e"):new Color("3b5365"),Size=bounds.Size};w.AddChild(panel);
        if(!backdrop)w.AddChild(new Label{Text=title+"\nOwned native test window",Position=new(18,18)});
        AddChild(w);w.Show();_owned.Add(w);return w;
    }
    private async Task Native()
    {
        var c=_app!.Controller;c.Automatic=false;var world=c.World;var area=world.Screens[0].Usable;
        var backdrop=Fixture("SCP platform test background",area,true);
        var a=Fixture("SCP platform fixture A",new(area.Position+new Vector2I(220,570),new(500,240)));
        var b=Fixture("SCP platform fixture B",new(area.Position+new Vector2I(820,470),new(460,230)));
        await Frames(12);
        foreach(var w in _owned)c.Observer.IncludedOwnedWindows.Add(Handle(w));
        c.Refresh();var observations=c.Observer.Read();string aid=c.Observer.IdFor(Handle(a))!,bid=c.Observer.IdFor(Handle(b))!;
        var ar=observations.Single(w=>w.Id==aid).Bounds;var br=observations.Single(w=>w.Id==bid).Bounds;
        File.WriteAllText(Path.Combine(_output,"fixture-observations.json"),JsonSerializer.Serialize(observations.Where(w=>w.Id==aid||w.Id==bid||w.Id==c.Observer.IdFor(Handle(backdrop))).Select(w=>new{w.Id,bounds=w.Bounds.ToString(),w.CanSupport,w.Blocks}),new JsonSerializerOptions{WriteIndented=true}));
        Require(area.Encloses(WindowsDesktop.GetBounds(a))&&area.Encloses(WindowsDesktop.GetBounds(b)),"all native fixture windows placed within requested monitor work area");
        Require(world.Surfaces.Any(s=>s.Id==aid)&&world.Surfaces.Any(s=>s.Id==bid),"DWM observation creates real window ledges");
        world.Place(new(ar.Position.X+250,ar.Position.Y-140));
        for(int i=0;i<55;i++){c.Advance(1d/60);await Frames(1);}
        Require(world.Body.SupportId==aid&&world.Body.Foot.Y==ar.Position.Y,"native observed ledge catches visible falling body");
        Require(WindowsDesktop.GetBounds(c.BodyWindow!).Position==(Vector2I)(world.Body.Foot-new Vector2(24,96)).Round(),"negative LG physical origin matches native actor position");
        var pixel=DisplayServer.ScreenGetPixel(WindowsDesktop.ToGodotPosition((Vector2I)world.Body.Foot-new Vector2I(0,25)));
        Require(Math.Abs(pixel.R-.353f)<.08&&Math.Abs(pixel.G-.784f)<.08&&Math.Abs(pixel.B-.855f)<.08,"actual desktop pixel shows physics body above owned test background");
        Trace("land",world);
        // Capture only our full-monitor backdrop and owned fixtures, never unrelated app content.
        using(var image=DisplayServer.ScreenGetImageRect(new Rect2I(WindowsDesktop.ToGodotPosition(ar.Position-new Vector2I(30,150)),new Vector2I(1080,470)))) image.SavePng(Path.Combine(_output,"native-platforms.png"));
        var before=world.Body.Foot;a.Position+=new Vector2I(40,-20);await Frames(8);c.Refresh();
        Require(world.Body.Foot==before+new Vector2(40,-20)&&world.Body.Motion==PlatformMotion.Balancing,"real native window movement carries support with balance event");
        for(int i=0;i<12;i++){c.Advance(1d/60);await Frames(1);}
        Require(world.Body.Foot.X<before.X+40,"native moved ledge creates inertial slip");Trace("move",world);
        a.Mode=Window.ModeEnum.Minimized;await Frames(12);c.Refresh();
        Require(world.Body.SupportId==null,"real minimize removes support");
        for(int i=0;i<90;i++){c.Advance(1d/60);await Frames(1);}
        Require(world.Body.SupportId!.StartsWith("floor:"),"real minimized ledge falls to requested monitor floor");Trace("minimize",world);
        a.Mode=Window.ModeEnum.Windowed;await Frames(12);c.Refresh();
        aid=c.Observer.IdFor(Handle(a))!;ar=c.Observer.Read().Single(w=>w.Id==aid).Bounds;
        world.Place(new(ar.End.X-60,ar.Position.Y-120));Step(world,.7);c.Publish();await Frames(6);
        Require(world.JumpToAdjacent(1),"native adjacent ledge accepts common jump command");
        for(int i=0;i<75;i++){c.Advance(1d/60);await Frames(1);}
        Require(world.Body.SupportId==bid,"visible jump lands on adjacent real window");Trace("jump",world);
        var cover=Fixture("SCP platform fixture cover",new(br.Position+new Vector2I(150,-120),new(200,220)));
        await Frames(10);c.Observer.IncludedOwnedWindows.Add(Handle(cover));c.Refresh();
        Require(world.Surfaces.Count(s=>s.Id==bid)==2,"actual foreground window splits covered native edge");Trace("cover",world);
        cover.Mode=Window.ModeEnum.Maximized;await Frames(12);c.Refresh();
        Require(world.Surfaces.All(s=>s.Floor),"actual maximized window removes obstructed ledges");
        cover.Hide();cover.QueueFree();await Frames(6);c.Refresh();
        world.Place(new(br.Position.X+100,br.Position.Y-120));Step(world,.7);c.Publish();
        Require(world.Body.SupportId==bid,"restored exposed native ledge is usable again");
        b.Hide();b.QueueFree();await Frames(6);c.Refresh();
        Require(world.Body.SupportId==null,"destroyed native window invalidates support identity");Trace("close",world);
        var revision=world.Revision;var position=world.Body.Foot;c.SetVisible(false);c.Advance(2);c.Refresh();await Frames(6);
        Require(world.Revision==revision&&world.Body.Foot==position&&!c.BodyWindow!.Visible,"hide freezes platform observations and physical display");
        c.SetVisible(true);await Frames(6);Require(c.BodyWindow!.Visible&&world.Revision>revision,"show refreshes environment and restores native display");
        ar=c.Observer.Read().Single(w=>w.Id==aid).Bounds;world.Place(new(ar.Position.X+200,ar.Position.Y-100));Step(world,.6);c.Publish();
        var automaticStart=world.Body.Foot;revision=world.Revision;
        c.Automatic=true;a.Position+=new Vector2I(20,0);await Frames(8);c.Automatic=false;
        Require(world.Revision>revision&&world.Body.Foot.X>automaticStart.X+10,"live controller polls moved OS window and advances shared physics");
        File.WriteAllText(Path.Combine(_output,"native-trace.json"),JsonSerializer.Serialize(_nativeTrace,new JsonSerializerOptions{WriteIndented=true}));
        File.WriteAllText(Path.Combine(_output,"monitor.json"),JsonSerializer.Serialize(new{requested=DesktopPlacement.RequestedMonitor,screen=area.ToString(),dpi=WindowsDesktop.GetDpi(c.BodyWindow!),hardware=WindowsMonitors.Read().Where(m=>m.X==area.Position.X&&m.Y==area.Position.Y)},new JsonSerializerOptions{WriteIndented=true}));
    }
    private void Trace(string action,PlatformWorld world)=>_nativeTrace.Add(new{action,x=world.Body.Foot.X,y=world.Body.Foot.Y,state=world.Body.Motion.ToString(),support=world.Body.SupportId,ledges=world.Surfaces.Count});
    public override async void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();for(int i=0;i+1<args.Length;i++)if(args[i]=="--output")_output=args[++i];
            if(_output==""||File.Exists(Path.Combine(_output,"results.json")))throw new InvalidOperationException("New evidence directory required.");Directory.CreateDirectory(_output);
            Logic();
            _app=GD.Load<PackedScene>("res://Scenes/App/PlatformDesktop.tscn").Instantiate<PlatformDesktop>();AddChild(_app);_app.Controller.Automatic=false;
            await Frames(5);bool native=DisplayServer.GetName()!="headless";
            if(native)await Native();
            _app.Controller.Shutdown();await Frames(4);
            Require(_app.Controller.World.Stopped&&_app.Controller.BodyWindow==null,"runtime shutdown releases platform actor");
            Cleanup();
            File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new{passed=true,native,checks=_checks.Count,tests=_checks,limits=new[]{"Generic physics marker; PLAN-005 SCP integration is on hold","Rectangular DWM bounds approximate irregular/translucent windows","Actual mixed-DPI hardware not tested"}},new JsonSerializerOptions{WriteIndented=true}));
            GD.Print("PLATFORM_SMOKE_PASS checks="+_checks.Count);GetTree().Quit();
        }
        catch(Exception ex){GD.PushError("PLATFORM_SMOKE_FAILED "+ex);_app?.Controller.Shutdown();Cleanup();GetTree().Quit(1);}
    }
    private void Cleanup(){foreach(var window in _owned)if(IsInstanceValid(window)){window.Hide();window.QueueFree();}_owned.Clear();}
}
