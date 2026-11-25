using Godot;
using SiteDesktop.Platforms;
using System;
using System.Linq;

namespace SiteDesktop.Behavior.Zombies;
public sealed class ZombieBehavior
{
    private readonly PlatformWorld _body;
    private readonly MovingState _moving;
    private readonly IdleState _idle;
    private MovementGoal? _goal;
    private int _direction=1;
    private double _wait;
    public StateMachine Machine { get; }=new();
    public MovementController Movement { get; }
    public ZombieBehavior(PlatformWorld body)
    {
        _body=body;Movement=new(body,new(1.5,60));
        _idle=new(body,dt=>_wait-=dt);
        _moving=new(Movement,()=>_goal,status=>
        {
            if(status==MovementStatus.Moving)return;
            _direction=-_direction;_goal=null;_wait=status==MovementStatus.Arrived?0:.5;Machine.Request(_idle,status.ToString());
        });Machine.Request(_idle,"created");
    }
    public void Tick(double delta)
    {
        if(_body.Body.Motion==PlatformMotion.Dragging){Movement.Cancel();_goal=null;Machine.Cancel();Machine.Request(_idle,"dragging");return;}
        if(_goal==null&&_wait<=0)
        {
            var area=_body.Screens.OrderBy(s=>((Vector2)s.Usable.GetCenter()).DistanceSquaredTo(_body.Body.Foot)).First().Usable;
            float left=area.End.X-Math.Min(250,area.Size.X/2),right=area.End.X-42;
            if(_body.Body.Foot.X<left)_direction=1;else if(_body.Body.Foot.X>right)_direction=-1;
            var floor=_body.Surfaces.First(s=>s.Floor&&s.WindowOrigin==(Vector2)area.Position);
            _goal=new(new(_direction>0?right:left,floor.Y),floor.Id,2);Machine.Request(_moving,"corner patrol");
        }
        Machine.Tick(delta);
    }
}
