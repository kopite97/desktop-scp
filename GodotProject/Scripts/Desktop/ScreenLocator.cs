using Godot;
using System.Collections.Generic;

namespace SiteDesktop.Desktop;
public static class ScreenLocator
{
    // Physical pixels; retain a previous valid screen only across a tie or a gap.
    public static DesktopScreen? Resolve(IReadOnlyList<DesktopScreen> screens,Rect2 footprint,DesktopScreen? previous=null)
    {
        DesktopScreen? best=null;float area=0;
        foreach(var screen in screens)
        {
            var overlap=footprint.Intersection(new Rect2(screen.Bounds.Position,screen.Bounds.Size));
            float value=overlap.Size.X*overlap.Size.Y;
            if(value>area||(value>0&&value==area&&screen==previous)){area=value;best=screen;}
        }
        if(best!=null)return best;
        foreach(var screen in screens)if(screen==previous)return screen;
        return null;
    }
    public static Rect2 Footprint(Vector2 foot,float half,float height)=>new(foot-new Vector2(half,height),new Vector2(half*2,height));
}
