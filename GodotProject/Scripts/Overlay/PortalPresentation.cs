using Godot;
using SiteDesktop.Behavior;
using System;
using System.Linq;

namespace SiteDesktop.Overlay;
public sealed class PortalPresentation(Node parent,OverlayStackCoordinator stack,string owner) : IDisposable
{
    private readonly ActorWindowLifetime _lifetime=new(stack,owner);
    private PortalEffectWindow? _departure,_arrival;
    public int WindowCount=>(_departure==null?0:1)+(_arrival==null?0:1);
    public PortalEffectWindow? Departure=>_departure;
    public PortalEffectWindow? Arrival=>_arrival;
    public void Publish(PortalFrame? frame,bool visible)
    {
        if(frame==null){Clear();return;}
        if(!visible){_departure?.Hide();_arrival?.Hide();stack.Remove(owner);return;}
        if(_departure==null){_departure=new();_arrival=new();_lifetime.Attach(parent,_departure);_lifetime.Attach(parent,_arrival);}
        _departure.Publish(frame.Departure,frame.Age,false);_arrival!.Publish(frame.Arrival,frame.Age,true);
        stack.Update(owner,new[]{_departure,_arrival}.Where(w=>w!.Visible).Select((w,i)=>new OverlayStackCoordinator.Entry(w!,35,i,false)));
    }
    private void Clear(){if(_departure!=null)_lifetime.Release(_departure);if(_arrival!=null)_lifetime.Release(_arrival);_departure=_arrival=null;stack.Remove(owner);}
    public void Dispose(){Clear();_lifetime.Dispose();}
}
