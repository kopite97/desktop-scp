using Godot;
using SiteDesktop.Desktop;
using System;
using System.Linq;

namespace SiteDesktop.Platforms;
public sealed class MonitorRoutePlanner(PlatformWorld body,PlatformNavigator navigator)
{
    public DesktopScreen? ScreenAt(Vector2 foot)=>ScreenLocator.Resolve(body.Screens,ScreenLocator.Footprint(foot,body.Settings.HalfWidth,body.Settings.Height));
    private MovementGoal? Gate(DesktopScreen here,DesktopScreen there)
    {
        var area=here.Usable;float margin=Math.Max(88,body.Settings.HalfWidth+24);
        if(area.Size.X<margin*2||area.Size.Y<Math.Max(160,body.Settings.Height+32))return null;
        float dx=there.Bounds.GetCenter().X-here.Bounds.GetCenter().X;
        float x=dx<0?area.Position.X+margin:dx>0?area.End.X-margin:area.GetCenter().X;
        return new(new(x,area.End.Y-8),$"floor:{here.Index}",6);
    }
    public MonitorRoute? Plan(MovementGoal target)
    {
        var from=ScreenAt(body.Body.Foot);var to=ScreenAt(target.Position);
        if(from==null||to==null||from==to)return null;
        var departure=Gate(from.Value,to.Value);var arrival=Gate(to.Value,from.Value);
        if(departure==null||arrival==null||navigator.Route(departure)==null||navigator.RouteFrom(arrival.Position,arrival.SupportId,target)==null)return null;
        return new(from.Value,to.Value,departure,arrival);
    }
    public bool Valid(MonitorRoute route,MovementGoal target)
        =>body.Screens.Contains(route.Source)&&body.Screens.Contains(route.Destination)&&ScreenAt(target.Position)==route.Destination
          &&navigator.RouteFrom(route.Arrival.Position,route.Arrival.SupportId,target)!=null;
}
