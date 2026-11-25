using System;
using SiteDesktop.Platforms;

namespace SiteDesktop.Behavior;
public sealed class IdleState(PlatformWorld body,Action<double> decide) : IBehaviorState
{
    public string Name=>"Idle";
    public void Enter()=>body.Walk(0);
    public void Tick(double delta){body.Walk(0);decide(delta);}
    public void Exit(){}
}
