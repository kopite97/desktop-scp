using Godot;
using SiteDesktop.App;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;
public partial class DesktopIconSmoke : Node
{
    private Main? _main;
    private readonly List<string> _checks=new();
    private string _output="";
    private void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);_checks.Add(text);GD.Print("ICON_CHECK "+text);}
    private async Task Frames(int count){for(int i=0;i<count;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private static bool Ring(DesktopIconImage i)
    {
        if(i.Width!=48||i.Height!=48)return false;
        int a=(8*48+8)*4,b=(24*48+24)*4;
        return i.Rgba[a]==240&&i.Rgba[a+1]==170&&i.Rgba[a+2]==60&&i.Rgba[a+3]==255&&i.Rgba[b+3]==0;
    }
    public override async void _Ready()
    {
        try
        {
            var args=OS.GetCmdlineUserArgs();for(int i=0;i+1<args.Length;i++)if(args[i]=="--output")_output=args[++i];
            if(_output==""||File.Exists(Path.Combine(_output,"results.json")))throw new InvalidOperationException("New evidence required");Directory.CreateDirectory(_output);
            var images=await DesktopIconProvider.ReadAsync(DesktopIconAppearance.WorkerExecutable,CancellationToken.None);
            Check(images.Length>0&&images.Length<=32,"Actual Desktop view returns bounded icon images");
            var fixture=images.SingleOrDefault(Ring);Check(fixture!=null,"Owned custom shortcut icon retrieved with exact RGB and transparent hole");
            using(var running=new CancellationTokenSource())
            {
                var read=DesktopIconProvider.ReadAsync(DesktopIconAppearance.WorkerExecutable,running.Token);running.Cancel();
                try{await read;Check(false,"Running cancellation must interrupt worker");}catch(OperationCanceledException){Check(true,"In-flight process cancellation completes without leaving worker");}
            }
            using(var cancelled=new CancellationTokenSource()){cancelled.Cancel();try{await DesktopIconProvider.ReadAsync(DesktopIconAppearance.WorkerExecutable,cancelled.Token);Check(false,"Cancelled request must fail");}catch(OperationCanceledException){Check(true,"Cancelled worker request terminates");}}
            await Lifecycle(fixture!);
            var elapsed=new List<double>();long memoryBefore=GC.GetTotalMemory(true);
            for(int i=0;i<3;i++)
            {
                var timer=System.Diagnostics.Stopwatch.StartNew();var again=await DesktopIconProvider.ReadAsync(DesktopIconAppearance.WorkerExecutable,CancellationToken.None);
                elapsed.Add(timer.Elapsed.TotalMilliseconds);Check(again.Any(Ring),"Repeated extraction retains fixture identity "+i);
            }
            int workers=0;foreach(var process in System.Diagnostics.Process.GetProcessesByName("DesktopIconWorker")){using(process)if(!process.HasExited)workers++;}
            Check(workers==0,"Bounded extraction and cancellation leave no worker processes");
            File.WriteAllText(Path.Combine(_output,"cost.json"),JsonSerializer.Serialize(new{extractionMilliseconds=elapsed,managedBytesBefore=memoryBefore,managedBytesAfter=GC.GetTotalMemory(true),remainingWorkers=workers,scope="Three sequential reads; not long-duration load"}));
            _main=GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();_main.PersistScpSettings=false;_main.EnableSystemOptions=false;_main.EnableStatue=false;AddChild(_main);
            _main.Companion.AutomaticInput=false;_main.Companion.SetVisible(false);
            var c=_main.Plague!;c.Automatic=false;c.State.AutoSpawn=false;
            for(int i=0;i<900&&_main.Icons!.CatalogCount==0;i++)await Frames(1);
            Check(_main.Icons!.CatalogCount>0,"Product Main owns populated runtime icon service");
            var image=_main.Icons.Find(fixture!.Id);Check(image!=null,"Owned fixture reaches product texture cache");
            using(var gpu=image!.Texture.GetImage())Check(gpu.GetData().SequenceEqual(fixture.Rgba),"Godot texture roundtrip preserves all source RGBA bytes");
            Check(image.Hit(new(16,16))&&!image.Hit(new(32,32))&&!image.Hit(new(0,0)),"Alpha hit mask includes body and excludes transparent hole and margin");
            foreach(int old in c.State.Objects.Keys.ToArray())c.State.Collect(old);
            c.State.ChooseAppearance=()=>fixture.Id;
            int id=c.State.SpawnAt(c.State.Doctor.Body.Foot+new Vector2(36,0))!.Value;
            for(int i=0;i<200;i++)c.Advance(.02);
            Check(c.State.Objects[id].Kind==DummyKind.Zombie&&c.State.Objects[id].AppearanceId==fixture.Id,"Treatment retains icon identity in zombie state");
            c.State.ChooseAppearance=()=>null;
            Check(c.State.Spawn()==null,"No icon means skip spawn instead of substitute art");
            c.State.ChooseAppearance=()=>fixture.Id;
            c.State.Collect(id);c.State.BeginDoctorDrag(c.State.Doctor.Body.Foot);c.State.Drag(c.State.Doctor.Body.Foot+new Vector2(-280,0));c.State.EndDrag();
            var area=c.State.Screens[0].Usable;
            id=c.State.SpawnAt(area.Position+new Vector2(800,area.Size.Y-20))!.Value;c.Publish();await Frames(20);
            Check(c.ObjectWindows[id].Icon==image,"Native actor receives cached icon appearance");
            using(var rendered=c.ObjectWindows[id].GetTexture().GetImage())
            {
                var color=rendered.GetPixel(16,16);Check(Math.Abs(color.R-240f/255)<.02&&Math.Abs(color.G-170f/255)<.02&&Math.Abs(color.B-60f/255)<.02,"Actual native viewport renders expected fixture color");
                Check(rendered.GetPixel(32,32).A==0,"Actual native viewport preserves fixture hole alpha");
                rendered.SavePng(Path.Combine(_output,"owned-fixture-render.png"));
            }
            if(args.Contains("--native-input"))await Input(c,id);
            c.SetVisible(false);_main.Icons.SetPaused(true);int refreshes=_main.Icons.SuccessfulRefreshes;await Frames(10);
            Check(!c.ObjectWindows.Values.Any(w=>w.Visible)&&_main.Icons.SuccessfulRefreshes==refreshes,"Hide pauses icons and removes native input islands");
            _main.Icons.SetPaused(false);c.SetVisible(true);await Frames(15);
            Check(c.ObjectWindows.Values.All(w=>w.Visible),"Restore shows existing icon snapshots");
            Check(_main.Icons.CachedCount<=46,"Catalog plus live appearance cache is bounded");
            _main.RequestExit();Check(_main.Icons.Stopped&&_main.Icons.CachedCount==0&&c.State.Stopped,"Exit clears textures and game state");
            File.WriteAllText(Path.Combine(_output,"results.json"),JsonSerializer.Serialize(new{passed=true,checks=_checks,nativeInput=args.Contains("--native-input"),catalogCount=images.Length,kinds=images.GroupBy(i=>i.Kind).Select(g=>new{kind=g.Key,count=g.Count()}),limitations=new[]{"Filename labels, thumbnails, sync badges not reproduced","Mixed-DPI hardware not yet verified","User visual confirmation pending"}},new JsonSerializerOptions{WriteIndented=true}));
            GD.Print("DESKTOP_ICON_PASS");
        }
        catch(Exception ex){GD.PushError("DESKTOP_ICON_FAILED "+ex);_main?.RequestExit();GetTree().Quit(1);}
    }
    private async Task Lifecycle(DesktopIconImage fixture)
    {
        // Failure and delayed results enter the same product service boundary, not a duplicate cache.
        int calls=0;var late=new TaskCompletionSource<DesktopIconImage[]>();
        var service=new DesktopIconAppearance{ReadImagesAsync=_=>++calls switch {1=>Task.FromResult(new[]{fixture}),2=>Task.FromException<DesktopIconImage[]>(new IOException("fixture disconnect")),3=>late.Task,_=>Task.FromResult(Array.Empty<DesktopIconImage>())}};
        AddChild(service);service.SetProcess(false);service._Process(0);service._Process(0);
        Check(service.CatalogCount==1,"Catalog sample accepted by product service");var snapshot=service.Find(fixture.Id);
        service._Process(30);service._Process(0);service.Retain(new[]{fixture.Id});
        Check(service.FailedRefreshes==1&&service.CatalogCount==0&&service.Find(fixture.Id)==snapshot,"Provider failure clears future candidates and retains live snapshot");
        service._Process(30);service.SetPaused(true);service.SetPaused(false);late.SetResult(new[]{fixture});service._Process(0);service._Process(0);
        Check(service.CatalogCount==0,"Completed stale generation is discarded after pause and resume");
        service.Retain(Array.Empty<string>());Check(service.CachedCount==0,"Unused snapshot released after object removal");
        service.Shutdown();service.QueueFree();await Frames(1);
    }
    private async Task Input(PlagueController c,int id)
    {
        using var fixture=new NativeInputFixture(_output);
        for(int i=0;i<300&&!fixture.Ready;i++)await Frames(1);
        Check(fixture.Ready&&fixture.Focus(),"Owned native receiver focused");await Frames(5);
        var center=WindowsDesktop.GetBounds(c.ObjectWindows[id]).Position+new Vector2I(32,32);
        // Receiver is on LG at its standard location; place our actor inside it.
        GD.Print("ICON_RECEIVER_READY");
        fixture.Move(center);c.Publish();await Frames(10);
        WindowsDesktop.TryGetPointer(out var actual,out _);
        GD.Print($"ICON_HOLE wanted={center} actual={actual} hit={c.ObjectWindows[id].ContainsDesktop(center)} pass={c.ObjectWindows[id].MousePassthrough} hwnd={NativeInputFixture.WindowAt(center)} receiver={File.ReadAllText(Path.Combine(_output,"receiver.json"))}");
        var counts=fixture.Counts();fixture.Click();await Frames(10);
        Check(fixture.Counts().Down==counts.Down+1&&fixture.Counts().Up==counts.Up+1,"Transparent icon hole passes actual click to receiver");
        var opaque=WindowsDesktop.GetBounds(c.ObjectWindows[id]).Position+new Vector2I(16,16);
        fixture.Move(opaque);c.Publish();await Frames(10);
        var before=c.State.Objects[id].Foot;c.Automatic=true;fixture.Press(true);await Frames(4);fixture.Move(opaque+new Vector2(80,-80));await Frames(8);
        Check(c.State.DraggedObject==id&&c.State.Objects[id].Foot.DistanceTo(before+new Vector2(80,-80))<2,"Native right drag moves the same icon clone");
        fixture.Release();await Frames(3);c.Automatic=false;Check(c.State.DraggedObject==null,"Native release ends icon drag");
        opaque=WindowsDesktop.GetBounds(c.ObjectWindows[id]).Position+new Vector2I(16,16);
        fixture.Move(opaque);c.Publish();await Frames(10);counts=fixture.Counts();fixture.Click();await Frames(10);
        Check(!c.State.Objects.ContainsKey(id)&&fixture.Counts().Down==counts.Down,"Opaque icon click collects clone without opening original");
    }
}
