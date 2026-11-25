using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Companions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
namespace SiteDesktop.Diagnostics;
public partial class Scp999Smoke
{
    private async Task NativePlatforms(Scp999Controller c)
    {
        var owned=new List<Window>();
        nint Handle(Window w)=>(nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,w.GetWindowId());
        var s=c.State;var area=s.Screens[0].Usable;
        Window Fixture(string title,Rect2I bounds,bool background=false)
        {
            var w=new Window{Visible=false,Title=title,ForceNative=true,Transient=false,AlwaysOnTop=true,Borderless=background,Position=WindowsDesktop.ToGodotPosition(bounds.Position),Size=bounds.Size};
            w.AddChild(new ColorRect{Color=new Color(background?"283747":"52697c"),Size=bounds.Size});
            if(!background)w.AddChild(new Label{Text=title,Position=new(20,20)});
            AddChild(w);w.Show();owned.Add(w);c.Observer.IncludedOwnedWindows.Add(Handle(w));return w;
        }
        void Step(double seconds){for(int i=0;i<(int)Math.Ceiling(seconds/.02);i++)c.Advance(.02,s.Foot);}
        try
        {
            Fixture("SCP-999 test background",area,true);
            var a=Fixture("SCP-999 platform A",new(area.Position+new Vector2I(220,570),new(500,240)));
            var b=Fixture("SCP-999 platform B",new(area.Position+new Vector2I(820,470),new(460,230)));
            await Frames(12);c.RefreshPlatforms();var observations=c.Observer.Read();
            string aid=c.Observer.IdFor(Handle(a))!,bid=c.Observer.IdFor(Handle(b))!;
            var ar=observations.Single(w=>w.Id==aid).Bounds;var br=observations.Single(w=>w.Id==bid).Bounds;
            Require(area.Encloses(WindowsDesktop.GetBounds(a))&&area.Encloses(WindowsDesktop.GetBounds(b)),"companion fixtures within requested LG work area");
            s.Physics.Place(new(ar.Position.X+220,ar.Position.Y-140));c.Advance(0,s.Foot);
            Require(s.Tag=="fall"&&!s.BeginPet(s.Foot),"native airborne companion rejects pet");
            for(int i=0;i<24;i++){c.Advance(1d/60,s.Foot);await Frames(1);}
            Require(s.Tag=="land"&&s.Physics.Body.SupportId==aid,"native observed collision plays landing");
            Step(.6);for(int i=0;i<30;i++)c.Advance(.02,s.Foot+new Vector2(200,0));await Frames(5);
            var trail=s.Effects.First(e=>e.Kind==CompanionEffectKind.Trail);var origin=trail.Origin;var foot=s.Foot;
            Require(trail.SupportId==aid&&WindowsDesktop.GetBounds(c.BodyWindow!).Position==(Vector2I)s.Position.Round(),"native body and trail attached to actual ledge at physical coordinates");
            using(var shot=DisplayServer.ScreenGetImageRect(new(WindowsDesktop.ToGodotPosition(ar.Position-new Vector2I(30,150)),new(1080,470))))shot.SavePng(Path.Combine(_output,"native-platforms.png"));
            a.Position+=new Vector2I(40,-20);await Frames(8);c.RefreshPlatforms();
            Require(s.Foot==foot+new Vector2(40,-20)&&trail.Origin==origin+new Vector2(40,-20)&&s.Tag=="balance","native moved window carries sprite and residue with balance animation");
            Step(.28);Require(s.Tag=="balance_recover","native balance recovery is not overwritten by idle");Step(.8);
            a.Mode=Window.ModeEnum.Minimized;await Frames(10);c.RefreshPlatforms();
            Require(s.Tag=="fall"&&!s.Effects.Any(e=>e.SupportId==aid),"actual minimize removes residue and releases companion");Step(2);
            Require(s.Physics.Body.SupportId!.StartsWith("floor:"),"native fall settles above LG taskbar");
            a.Mode=Window.ModeEnum.Windowed;await Frames(10);c.RefreshPlatforms();aid=c.Observer.IdFor(Handle(a))!;ar=c.Observer.Read().Single(w=>w.Id==aid).Bounds;
            s.Physics.Place(new(ar.End.X-90,ar.Position.Y-100));Step(.8);
            Require(s.Physics.JumpToAdjacent(1),"actual adjacent window accepts companion jump");c.Advance(0,s.Foot);Require(s.Tag=="takeoff","native jump starts takeoff");
            for(int i=0;i<75;i++){c.Advance(1d/60,s.Foot);await Frames(1);}
            Require(s.Physics.Body.SupportId==bid,"visible companion lands on neighboring native window");
            for(int i=0;i<30;i++)c.Advance(.02,s.Foot+new Vector2(200,0));Require(s.Effects.Any(e=>e.SupportId==bid),"neighbor window receives its own trail");
            var cover=Fixture("SCP-999 cover",new(br.Position-new Vector2I(0,150),new(500,250)));await Frames(10);c.RefreshPlatforms();
            Require(s.Physics.Body.SupportId==null&&!s.Effects.Any(e=>e.SupportId==bid),"native occlusion clears companion support and residue");cover.Hide();await Frames(4);c.RefreshPlatforms();
            s.Physics.Place(new(br.Position.X+180,br.Position.Y-100));Step(.8);for(int i=0;i<30;i++)c.Advance(.02,s.Foot+new Vector2(200,0));
            b.Hide();b.QueueFree();await Frames(6);c.RefreshPlatforms();
            Require(s.Tag=="fall"&&!s.Effects.Any(e=>e.SupportId==bid),"destroyed native support clears residue without stale identity");
            s.Physics.Place(new(ar.Position.X+200,ar.Position.Y-100));Step(.8);var revision=s.Physics.Revision;
            c.SetVisible(false);a.Position+=new Vector2I(20,0);await Frames(4);c.RefreshPlatforms();Require(s.Physics.Revision==revision,"hidden companion freezes environment observation");
            c.SetVisible(true);Require(s.Physics.Revision>revision&&s.Tag=="balance","restore observes moved support immediately");
            Step(.8);revision=s.Physics.Revision;c.AutomaticInput=true;a.Position+=new Vector2I(20,0);await Frames(8);c.AutomaticInput=false;
            Require(s.Physics.Revision>revision,"product controller polls actual OS windows");
            File.WriteAllText(Path.Combine(_output,"platform-monitor.json"),JsonSerializer.Serialize(new{requested=DesktopPlacement.RequestedMonitor,area=area.ToString(),dpi=WindowsDesktop.GetDpi(c.BodyWindow!),support=s.Physics.Body.SupportId,tag=s.Tag},new JsonSerializerOptions{WriteIndented=true}));
        }
        finally{c.AutomaticInput=false;c.Observer.IncludedOwnedWindows.Clear();foreach(var w in owned)if(IsInstanceValid(w)){w.Hide();w.QueueFree();}}
        await Frames(5);c.RefreshPlatforms();c.ResetPositions();
    }
}
