using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Platforms;
public sealed class MovementController
{
    private readonly PlatformWorld _body;
    private readonly MovementSettings _settings;
    public PlatformNavigator Navigator { get; }
    public MovementStatus Status { get; private set; }=MovementStatus.Cancelled;
    private IReadOnlyList<PlatformSurface>? _route;
    private long _revision=-1;
    private string? _support,_targetSupport;
    private Godot.Vector2? _lastGoal;
    private double _age,_stuck,_budget;
    private float _lastDistance=float.PositiveInfinity;
    public MovementController(PlatformWorld body,MovementSettings? settings=null){_body=body;_settings=settings??new();Navigator=new(body);}
    public bool CanReach(MovementGoal goal)=>Navigator.Route(goal)!=null;
    public void Reset(){_revision=-1;_age=_stuck=_budget=0;_lastDistance=float.PositiveInfinity;_route=null;_lastGoal=null;}
    public void Cancel(){_body.Walk(0);Status=MovementStatus.Cancelled;}
    public MovementStatus Update(MovementGoal goal,double delta)
    {
        var b=_body.Body;_body.Walk(0);_body.Body.AutoJump=false;
        if(b.Motion==PlatformMotion.Dragging)return Status=MovementStatus.Cancelled;
        if(!_body.Visible||_body.Stopped)return Status=MovementStatus.Cancelled;
        if(_budget==0)
        {
            var path=Navigator.Route(goal);float pathDistance=b.Foot.DistanceTo(goal.Position);
            if(path!=null&&path.Count>1)pathDistance+=path.Sum(s=>s.Right-s.Left);
            _budget=_settings.DistanceBudget?Math.Clamp(pathDistance/Math.Max(1,_body.Settings.Speed*_body.SpeedScale)*2+4,_settings.TimeoutSeconds,180):_settings.TimeoutSeconds;
        }
        _age+=delta;if(_age>=_budget)return Status=MovementStatus.Blocked;
        if(b.SupportId==null||b.Motion!=PlatformMotion.Grounded)return Status=MovementStatus.Moving;
        if(b.SupportId==goal.SupportId&&Math.Abs(b.Foot.X-goal.Position.X)<=goal.Tolerance&&Math.Abs(b.Foot.Y-goal.Position.Y)<2)return Status=MovementStatus.Arrived;
        if(_revision!=_body.Revision||_support!=b.SupportId||_targetSupport!=goal.SupportId)
        {
            _route=Navigator.Route(goal);_revision=_body.Revision;_support=b.SupportId;_targetSupport=goal.SupportId;_stuck=0;_lastDistance=float.PositiveInfinity;
        }
        if(_route==null)return Status=MovementStatus.Unreachable;
        float targetX=goal.Position.X;
        if(_route.Count>1)
        {
            var current=_route[0];var next=_route[1];
            if(PlatformNavigator.CanDrop(current,next,_body.Settings.HalfWidth))
            {
                float left=current.Left-_body.Settings.HalfWidth-2,right=current.Right+_body.Settings.HalfWidth+2;
                targetX=next.Contains(left,_body.Settings.HalfWidth)&&(!next.Contains(right,_body.Settings.HalfWidth)||Math.Abs(left-b.Foot.X)<Math.Abs(right-b.Foot.X))?left:right;
            }
            else
            {
                if(_body.TryJumpTo(next))return Status=MovementStatus.Moving;
                targetX=Math.Clamp((next.Left+next.Right)/2,current.Left+_body.Settings.HalfWidth,current.Right-_body.Settings.HalfWidth);
            }
        }
        float distance=Math.Abs(targetX-b.Foot.X);
        bool targetMoved=_lastGoal is {} last&&last.DistanceSquaredTo(goal.Position)>1;_lastGoal=goal.Position;
        if(distance<_lastDistance-.5f||(targetMoved&&_body.WalkDistance>.1f)){_stuck=0;_lastDistance=distance;}else _stuck+=delta;
        if(_stuck>=_settings.StuckSeconds)return Status=MovementStatus.Blocked;
        _body.Walk(Math.Sign(targetX-b.Foot.X));return Status=MovementStatus.Moving;
    }
}
