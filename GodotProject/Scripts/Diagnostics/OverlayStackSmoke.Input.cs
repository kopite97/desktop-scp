using Godot;
using SiteDesktop.Companions;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;
public partial class OverlayStackSmoke
{
    private async Task NativeOverlap()
    {
        _background!.Hide();
        var c=_main.Companion;var p=_main.Plague!;
        var area=c.State.Screens[0].Usable;
        _center=new(area.Position.X+1150,area.End.Y-8);
        _capture=new(WindowsDesktop.ToGodotPosition((Vector2I)_center-new Vector2I(220,180)),new(440,185));
        using var fixture=new NativeInputFixture(_output);
        for(int i=0;i<180&&!fixture.Ready;i++)await Frames(1);
        Require(fixture.Ready,"overlap receiver ready");
        bool focused=false;for(int i=0;i<30&&!focused;i++){focused=fixture.Focus();await Frames(1);}
        Require(focused,"overlap receiver foreground verified");
        async Task Drag(string kind,int? id=null)
        {
            c.State.EndGesture();p.State.EndDrag();c.State.Physics.Place(_center);p.State.Doctor.Place(_center);
            Vector2 start=_center+new Vector2(150,0);
            bool companion=kind=="999";
            if(companion)c.State.Physics.Place(start);
            else if(id is int item)p.State.Objects[item].Physics.Place(start);
            else p.State.Doctor.Place(start);
            c.Publish();p.Publish();await Frames(6);
            var window=companion?(Window)c.BodyWindow!:id is int actor?p.ObjectWindows[actor]:p.DoctorWindow!;
            Vector2 point=start-new Vector2(0,22);
            if(window is SiteDesktop.Overlay.PlagueActorWindow actorWindow)
            {
                // Approved sprites have alpha holes; the former polygon-center point is not a contract.
                var local=Enumerable.Range(0,window.Size.Y).SelectMany(y=>Enumerable.Range(0,window.Size.X).Select(x=>new Vector2(x+.5f,y+.5f)))
                    .Where(actorWindow.Hit).OrderBy(v=>v.DistanceSquaredTo((Vector2)window.Size/2)).First();
                point=WindowsDesktop.GetBounds(window).Position+local;
            }
            fixture.Move(point);c.Publish();p.Publish();await Frames(4);
            Require(NativeInputFixture.WindowAt(point)==Handle(window),kind+" native drag starts on correct actor");
            c.AutomaticInput=companion;p.Automatic=!companion;
            fixture.Press(true);await Frames(5);
            Require(companion?c.State.Gesture==CompanionGesture.Drag:id is int itemId?p.State.DraggedObject==itemId:p.State.Phase==DoctorPhase.Dragging,kind+" real right press starts product gesture");
            for(int i=1;i<=18;i++)
            {
                var wanted=point-new Vector2(i*10,0);fixture.Move(wanted);await Frames(3);
                WindowsDesktop.TryGetPointer(out var actual,out _);
                Require(((Vector2)actual).DistanceTo(wanted)<2,kind+" cursor held on trajectory "+i);
                var others=new Window[]{c.BodyWindow!,p.DoctorWindow!}.Where(w=>w!=window).Select(w=>(nint)Handle(w));
                // Only the first window must be above all other actors; their base order is tested separately.
                foreach(var other in others)Require(WindowsDesktop.StackMatches(new[]{(nint)Handle(window),other}),kind+" stays ahead while overlapping "+i);
                _frame++;Capture("native-"+kind);Sample("native-"+kind,"drag");
            }
            fixture.Release();await Frames(5);c.AutomaticInput=false;p.Automatic=false;
            Require(c.State.Gesture!=CompanionGesture.Drag&&p.State.DraggedObject==null&&p.State.Phase!=DoctorPhase.Dragging,kind+" actual release ends gesture");
        }
        await Drag("999");await Drag("049");
        int data=p.State.SpawnAt(_center+new Vector2(150,0))!.Value;p.Publish();await Drag("data",data);
        p.State.Collect(data);p.State.ResetDoctor();int zombie=p.State.SpawnAt(p.State.Doctor.Body.Foot+new Vector2(35,0))!.Value;
        for(int i=0;i<190;i++)p.Advance(.02);
        Require(p.State.Objects[zombie].Kind==DummyKind.Zombie,"product treatment creates zombie for drag test");
        await Drag("zombie",zombie);
        var target=p.State.Objects[zombie].Foot-new Vector2(0,22);c.SetVisible(false);p.State.Doctor.Place(_center+new Vector2(-150,0));p.Publish();await Frames(5);
        fixture.Move(target);p.Publish();await Frames(3);var counts=fixture.Counts();fixture.Press();await Frames(3);fixture.Release();await Frames(5);
        Require(!p.State.Objects.ContainsKey(zombie)&&fixture.Counts().Down==counts.Down,"front zombie click collects without desktop input");
        p.SetVisible(false);await Frames(4);fixture.Move(_center);counts=fixture.Counts();fixture.Click();await Frames(5);
        Require(fixture.Counts().Down==counts.Down+1&&fixture.Counts().Up==counts.Up+1,"hidden overlap area passes click to receiver");
        p.SetVisible(true);c.SetVisible(true);
    }
}
