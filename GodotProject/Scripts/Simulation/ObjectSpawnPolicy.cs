using Godot;
using SiteDesktop.Desktop;
using System.Collections.Generic;

namespace SiteDesktop.Simulation;
public sealed record SpawnAnchor(Rect2 Footprint,bool Suspended=false);
public sealed class ObjectSpawnPolicy
{
    private DesktopScreen? _previous;
    public DesktopScreen? Select(IReadOnlyList<DesktopScreen> screens,SpawnAnchor? anchor)
    {
        if(anchor==null){_previous=null;return null;}
        if(anchor.Suspended)return null;
        var result=ScreenLocator.Resolve(screens,anchor.Footprint,_previous);
        _previous=result;return result;
    }
}
