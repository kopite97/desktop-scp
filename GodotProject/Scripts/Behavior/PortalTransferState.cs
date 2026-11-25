using Godot;
using SiteDesktop.Platforms;
using System;

namespace SiteDesktop.Behavior;
public sealed record PortalFrame(long Id,Vector2 Departure,Vector2 Arrival,double Age,float BodyAlpha,bool Committed);
// Time and position have one owner. Presentation consumes Frame and never advances it.
public sealed class PortalTransferState(PlatformWorld body,Func<MonitorRoute,bool> valid,Action<bool> completed,PortalTransferSettings? settings=null) : IBehaviorState
{
    public string Name=>"Transferring";
    public PortalTransferSettings Settings { get; }=settings??new();
    public MonitorRoute? Route { get; private set; }
    public bool Active { get; private set; }
    public bool Committed { get; private set; }
    public double Age { get; private set; }
    public long Generation { get; private set; }
    public int Commits { get; private set; }
    private bool _finished;
    public PortalFrame? Frame=>!Active||Route==null?null:new(Generation,Route.Departure.Position,Route.Arrival.Position,Age,
        (float)(Age<Settings.DepartureSeconds?1-Age/Settings.DepartureSeconds:Age<Settings.DepartureSeconds+Settings.HiddenSeconds?0:Math.Clamp((Age-Settings.DepartureSeconds-Settings.HiddenSeconds)/Settings.ArrivalSeconds,0,1)),Committed);
    public void Prepare(MonitorRoute route)=>Route=route;
    public void Enter(){Generation++;_finished=false;Age=0;Committed=false;Active=true;body.Walk(0);}
    public void Tick(double delta)
    {
        if(!Active||_finished)return;
        if(Route==null||!valid(Route)){Finish(false);return;}
        Age+=delta;
        if(!Committed&&Age>=Settings.DepartureSeconds)
        {body.Place(Route.Arrival.Position);body.Walk(0);Committed=true;Commits++;}
        if(Age>=Settings.Duration)Finish(true);
    }
    private void Finish(bool success){if(_finished)return;_finished=true;completed(success);}
    public void Exit(){Active=false;Route=null;Generation++;}
}
