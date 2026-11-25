using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Platforms;
using SiteDesktop.Simulation;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
namespace SiteDesktop.Diagnostics;
public partial class Scp049Smoke
{
    private static nint Handle(Window w)=>(nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,w.GetWindowId());
    private Window Fixture(string title,Rect2I bounds,bool background=false)
    {
        var w=new Window{Title=title,Visible=false,ForceNative=true,Transient=false,AlwaysOnTop=true,Borderless=background,Position=WindowsDesktop.ToGodotPosition(bounds.Position),Size=bounds.Size};
        w.AddChild(new ColorRect{Color=new Color(background?"263440":"4c6477"),Size=bounds.Size});
        if(!background)w.AddChild(new Label{Text=title+" — owned test window",Position=new(18,18)});
        AddChild(w);w.Show();_owned.Add(w);return w;
    }
    private static void Clear(PlagueController c){foreach(int id in c.State.Objects.Keys.ToArray())c.State.Collect(id);c.Publish();}
    private static void Advance(PlagueController c,double seconds){for(int i=0;i<(int)Math.Ceiling(seconds/.02);i++)c.Advance(.02);}
    private async Task Native(PlagueController c,bool input)
    {
        await Frames(20);
        Require(_main!.GetNode<VBoxContainer>("Scroll/Content").GetRect().End.Y<=GetWindow().Size.Y,"all application controls fit inside visible panel");
        using(var panel=GetWindow().GetTexture().GetImage())panel.SavePng(Path.Combine(_output,"control-panel.png"));
        var s=c.State;Clear(c);_main.Companion.SetVisible(false);var area=s.Screens[0].Usable;
        var bg=Fixture("SCP-049 test background",area,true);
        var a=Fixture("SCP-049 platform A",new(area.Position+new Vector2I(220,570),new(560,240)));
        await Frames(10);foreach(var w in _owned)c.Observer.IncludedOwnedWindows.Add(Handle(w));c.Refresh();
        string aid=c.Observer.IdFor(Handle(a))!;var bounds=c.Observer.Read().Single(w=>w.Id==aid).Bounds;
        s.Doctor.Place(new(bounds.Position.X+230,bounds.Position.Y-120));int id=s.SpawnAt(new(bounds.Position.X+265,bounds.Position.Y-120))!.Value;
        for(int i=0;i<55;i++){c.Advance(1d/60);await Frames(1);}
        Require(s.Phase==DoctorPhase.Treating&&s.Doctor.Body.SupportId==aid,"native falling doctor and data start surgery on actual window");
        Require(c.DoctorWindow!.Visible&&c.DoctorWindow.Transparent&&c.DoctorWindow.Unfocusable&&c.ObjectWindows[id].Visible,"native doctor and data use separate transparent unfocusable windows");
        Require(WindowsDesktop.GetBounds(c.DoctorWindow).Position==(Vector2I)(s.Doctor.Body.Foot-new Vector2(64,112)).Round(),"native LG physical coordinates match doctor simulation");
        Require(c.DoctorWindow.ContainsDesktop(s.Doctor.Body.Foot-new Vector2(0,35))&&!c.DoctorWindow.ContainsDesktop(s.Doctor.Body.Foot-new Vector2(62,110)),"temporary doctor silhouette distinguishes body and empty corner");
        using(var image=DisplayServer.ScreenGetImageRect(new(WindowsDesktop.ToGodotPosition(bounds.Position-new Vector2I(30,150)),new(700,420))))image.SavePng(Path.Combine(_output,"native-treatment.png"));
        var before=s.Doctor.Body.Foot;var dataBefore=s.Objects[id].Foot;a.Position+=new Vector2I(40,-20);await Frames(8);c.Refresh();
        Require(s.Doctor.Body.Foot==before+new Vector2(40,-20)&&s.Objects[id].Foot==dataBefore+new Vector2(40,-20)&&s.TargetId==null,"actual window movement carries both and interrupts treatment");
        Advance(c,.6);Require(s.Phase==DoctorPhase.Treating,"native stable support restarts treatment");
        a.Mode=Window.ModeEnum.Minimized;await Frames(8);c.Refresh();
        Require(s.TargetId==null&&s.Doctor.Body.SupportId==null,"actual minimize cancels surgery and releases support");Advance(c,1.2);
        Require(s.Doctor.Body.SupportId!.StartsWith("floor:")&&s.Objects[id].Physics.Body.SupportId!.StartsWith("floor:"),"native doctor and data settle above LG taskbar");
        Clear(c);s.ResetDoctor();id=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(35,0))!.Value;c.Publish();
        int previous=s.Transformations;
        for(int i=0;i<220;i++){c.Advance(1d/60);await Frames(1);}
        Require(s.Transformations==previous+1&&s.Objects[id].Kind==DummyKind.Zombie&&c.ObjectWindows.ContainsKey(id),"visible real-frame treatment changes data to zombie exactly once");
        using(var image=DisplayServer.ScreenGetImageRect(new(WindowsDesktop.ToGodotPosition(area.Position+new Vector2I(280,area.Size.Y-170)),new(620,165))))image.SavePng(Path.Combine(_output,"native-zombie.png"));
        double age=s.Objects[id].Age,clock=s.Clock;c.SetVisible(false);await Frames(5);c.Advance(10);
        Require(!c.DoctorWindow.Visible&&c.ObjectWindows.Values.All(w=>!w.Visible)&&s.Clock==clock&&s.Objects[id].Age==age,"hide removes every native input island and freezes zombie age");
        c.SetVisible(true);await Frames(4);Require(c.DoctorWindow.Visible&&c.ObjectWindows.Values.All(w=>w.Visible),"restore recreates visible native windows without replaying hidden time");
        Require(s.Collect(id),"common collection removes native zombie");c.Publish();await Frames(4);Require(!c.ObjectWindows.ContainsKey(id),"removed zombie frees native window");
        // Move a surviving support while hidden, then remove it before show.
        a.Mode=Window.ModeEnum.Windowed;await Frames(8);c.Refresh();aid=c.Observer.IdFor(Handle(a))!;bounds=c.Observer.Read().Single(w=>w.Id==aid).Bounds;
        s.Doctor.Place(new(bounds.Position.X+230,bounds.Position.Y-100));id=s.SpawnAt(new(bounds.Position.X+265,bounds.Position.Y-100))!.Value;Advance(c,.8);
        c.SetVisible(false);a.Hide();a.QueueFree();await Frames(5);c.SetVisible(true);
        Require(s.Doctor.Body.SupportId==null&&s.TargetId==null,"restore validates destroyed native ledge and cancels stale operation");
        Clear(c);s.ResetDoctor();c.Observer.IncludedOwnedWindows.Clear();foreach(var w in _owned)if(IsInstanceValid(w)){w.Hide();w.QueueFree();}_owned.Clear();await Frames(5);c.Refresh();
        File.WriteAllText(Path.Combine(_output,"monitor.json"),JsonSerializer.Serialize(new{monitor=DesktopPlacement.RequestedMonitor,workArea=area.ToString(),dpi=WindowsDesktop.GetDpi(c.DoctorWindow)},new JsonSerializerOptions{WriteIndented=true}));
        if(input)await NativeInput(c);
    }
    private async Task NativeInput(PlagueController c)
    {
        WindowsDesktop.SetControlVisible(GetWindow(),false);
        var s=c.State;s.AutoSpawn=false;Clear(c);s.ResetDoctor();
        using var fixture=new NativeInputFixture(_output);
        for(int i=0;i<180&&!fixture.Ready;i++)await Frames(1);Require(fixture.Ready,"owned separate input receiver ready");
        bool focused=false;for(int i=0;i<30&&!focused;i++){focused=fixture.Focus();await Frames(1);}Require(focused,"owned input receiver foreground verified");await Frames(10);c.Refresh();
        var top=s.Doctor.Body.Foot-new Vector2(64,112);fixture.Move(top+new Vector2(2,2));c.Publish();await Frames(10);var counts=fixture.Counts();fixture.Click();await Frames(10);
        Require(fixture.Counts().Down==counts.Down+1&&fixture.Counts().Up==counts.Up+1,"doctor empty corner passes actual input to separate process");
        var point=s.Doctor.Body.Foot-new Vector2(0,30);fixture.Move(point);c.Publish();await Frames(10);var start=s.Doctor.Body.Foot;
        Require(NativeInputFixture.WindowAt(point)==(long)Handle(c.DoctorWindow!),"native pointer targets owned doctor input island");
        c.Automatic=true;fixture.Press(true);await Frames(10);fixture.Move(point+new Vector2(120,-150));c.Publish();await Frames(6);
        Require(s.Phase==DoctorPhase.Dragging&&s.Doctor.Body.Foot.DistanceTo(start+new Vector2(120,-150))<2,"native right drag moves doctor through product input");
        fixture.Release();await Frames(10);c.Automatic=false;Require(s.Phase!=DoctorPhase.Dragging&&s.Doctor.Body.Motion==PlatformMotion.Falling,"native right release drops doctor with gravity");Advance(c,1);
        int id=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(180,0))!.Value;Advance(c,.4);var item=s.Objects[id];point=item.Foot-new Vector2(0,20);fixture.Move(point);c.Publish();await Frames(10);start=item.Foot;
        Require(NativeInputFixture.WindowAt(point)==(long)Handle(c.ObjectWindows[id]),"native pointer targets owned data input island");
        c.Automatic=true;fixture.Press(true);await Frames(10);fixture.Move(point+new Vector2(80,-100));c.Publish();await Frames(6);
        WindowsDesktop.TryGetPointer(out var actualPointer,out _);GD.Print($"DATA_DRAG_RESULT wanted={point+new Vector2(80,-100)} actualPointer={actualPointer} dragged={s.DraggedObject} foot={item.Foot} expectedFoot={start+new Vector2(80,-100)} right={WindowsDesktop.RightPressed}");
        Require(s.DraggedObject==id&&item.Foot.DistanceTo(start+new Vector2(80,-100))<2,"native data drag releases doctor reservation and moves same object");fixture.Release();await Frames(10);c.Automatic=false;Advance(c,.8);
        fixture.Move(item.Foot-new Vector2(0,20));c.Publish();await Frames(10);counts=fixture.Counts();
        GD.Print($"DATA_CLICK_TARGET actual={NativeInputFixture.WindowAt(item.Foot-new Vector2(0,20))} expected={Handle(c.ObjectWindows[id])} point={item.Foot-new Vector2(0,20)} bounds={WindowsDesktop.GetBounds(c.ObjectWindows[id])}");
        fixture.Press();await Frames(3);fixture.Release();await Frames(10);
        GD.Print($"DATA_CLICK_RESULT exists={s.Objects.ContainsKey(id)} window={c.ObjectWindows.ContainsKey(id)} before={counts} after={fixture.Counts()}");
        Require(!s.Objects.ContainsKey(id)&&!c.ObjectWindows.ContainsKey(id)&&fixture.Counts().Down==counts.Down,"native data click collects game object without reaching desktop receiver");
        Clear(c);s.ResetDoctor();id=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(35,0))!.Value;Advance(c,3.6);Require(s.Objects[id].Kind==DummyKind.Zombie,"native input fixture has transformed zombie");
        point=s.Objects[id].Foot-new Vector2(0,22);fixture.Move(point);c.Publish();await Frames(10);counts=fixture.Counts();int collected=s.Collected;fixture.Click();await Frames(10);
        Require(!s.Objects.ContainsKey(id)&&s.Collected==collected+1&&fixture.Counts().Down==counts.Down,"native zombie click collects exactly once");
        _main!.Companion.SetVisible(true);_main.Companion.ResetPositions();s.ResetDoctor();await Frames(10);c.Refresh();
        var companion=_main.Companion;point=companion.State.Position+new Vector2(64,90);fixture.Move(point);c.Publish();companion.AutomaticInput=true;await Frames(10);float stress=s.DoctorStress.Value;counts=fixture.Counts();fixture.Press();await Frames(3);fixture.Release();await Frames(10);companion.AutomaticInput=false;
        Require(s.DoctorStress.Value==stress-3&&fixture.Counts().Down==counts.Down,"native SCP-999 pet reduces actual SCP-049 shared stress");companion.State.EndGesture();companion.SetVisible(false);
        id=s.SpawnAt(s.Doctor.Body.Foot+new Vector2(180,0))!.Value;Advance(c,.4);point=s.Objects[id].Foot-new Vector2(0,22);c.SetVisible(false);await Frames(10);fixture.Move(point);c.Publish();await Frames(10);counts=fixture.Counts();fixture.Click();await Frames(10);
        Require(s.Objects.ContainsKey(id)&&fixture.Counts().Down==counts.Down+1,"hidden data cannot intercept or consume clicks");c.SetVisible(true);
    }
}
