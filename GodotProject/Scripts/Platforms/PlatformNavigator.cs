using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Platforms;

// Bounded graph over the actual one-way platforms. Edges use the physics solver's
// landing check, not distance alone. Unsupported routes return failure.
public sealed class PlatformNavigator(PlatformWorld body)
{
    public int Searches { get; private set; }
    private long _revision=-1;
    private readonly Dictionary<(PlatformSurface Source,PlatformSurface Target),IReadOnlyList<PlatformSurface>?> _routes=new();
    public IReadOnlyList<PlatformSurface>? Route(MovementGoal goal)
        =>RouteFrom(body.Body.Foot,body.Body.SupportId,goal);
    public IReadOnlyList<PlatformSurface>? RouteFrom(Vector2 foot,string? supportId,MovementGoal goal)
    {
        if(_revision!=body.Revision){_routes.Clear();_revision=body.Revision;}
        var source=body.Surfaces.FirstOrDefault(s=>s.Id==supportId&&s.Contains(foot.X,body.Settings.HalfWidth));
        var target=body.Surfaces.FirstOrDefault(s=>s.Id==goal.SupportId&&s.Contains(goal.Position.X,body.Settings.HalfWidth));
        if(source==null||target==null)return null;
        if(_routes.TryGetValue((source,target),out var cached))return cached;
        Searches++;
        var route=Search(source,target);
        if(_routes.Count>=128)_routes.Clear();
        _routes[(source,target)]=route;
        return route;
    }
    private IReadOnlyList<PlatformSurface>? Search(PlatformSurface source,PlatformSurface target)
    {
        var queue=new Queue<PlatformSurface>();var previous=new Dictionary<PlatformSurface,PlatformSurface?>{{source,null}};queue.Enqueue(source);
        while(queue.Count>0&&previous.Count<=128)
        {
            var current=queue.Dequeue();
            if(current==target)
            {
                var path=new List<PlatformSurface>();for(PlatformSurface? s=target;s!=null;s=previous[s])path.Add(s);path.Reverse();return path;
            }
            foreach(var next in body.Surfaces)
            {
                if(previous.ContainsKey(next))continue;
                float x=Math.Clamp((next.Left+next.Right)/2,current.Left+body.Settings.HalfWidth,current.Right-body.Settings.HalfWidth);
                if(CanDrop(current,next,body.Settings.HalfWidth)||body.JumpSolution(new(x,current.Y),next,out _,out _))
                {previous[next]=current;queue.Enqueue(next);}
            }
        }
        return null;
    }
    public static bool CanDrop(PlatformSurface from,PlatformSurface to,float half)
        =>!from.Floor&&to.Y>from.Y+1&&(to.Contains(from.Left-half-2,half)||to.Contains(from.Right+half+2,half));
}
