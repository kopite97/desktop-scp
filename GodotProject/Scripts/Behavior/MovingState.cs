using System;
using SiteDesktop.Platforms;

namespace SiteDesktop.Behavior;
public sealed class MovingState(MovementController movement,Func<MovementGoal?> goal,Action<MovementStatus> result) : IBehaviorState
{
    public string Name=>"Moving";
    public void Enter()=>movement.Reset();
    public void Tick(double delta)
    {
        if(goal() is not {} target){movement.Cancel();result(MovementStatus.Cancelled);return;}
        result(movement.Update(target,delta));
    }
    public void Exit()=>movement.Cancel();
}
