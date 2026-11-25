using Godot;
using SiteDesktop.Companions;
using SiteDesktop.Platforms;
using System;
using System.Linq;

namespace SiteDesktop.Behavior.Scp999;
public sealed class CompanionBehavior
{
    private readonly PlatformWorld _body;
    private readonly Scp999Settings _settings;
    private readonly Random _random;
    private readonly IdleState _idle;
    private readonly MovingState _moving;
    private MovementGoal? _goal;
    private double _wait,_retry;
    private bool _following;
    public StateMachine Machine { get; }=new();
    public MovementController Movement { get; }
    public CompanionBehavior(PlatformWorld body,Scp999Settings settings,int seed=999)
    {
        _body=body;_settings=settings;_random=new(seed);Movement=new(body);
        _idle=new(body,delta=>_wait-=delta);
        _moving=new(Movement,()=>_goal,result=>
        {
            if(result==MovementStatus.Moving)return;
            _retry=result is MovementStatus.Blocked or MovementStatus.Unreachable?.8:0;
            _goal=null;_wait=Pause();Machine.Request(_idle,result.ToString());
        });Reset();
    }
    private double Pause()=>_settings.IdleMinSeconds+_random.NextDouble()*(_settings.IdleMaxSeconds-_settings.IdleMinSeconds);
    public void Reset(){Machine.Cancel();Movement.Cancel();_goal=null;_following=false;_retry=0;_wait=Pause();Machine.Request(_idle,"reset");}
    public void Tick(double delta,Vector2? pointer)
    {
        _retry=Math.Max(0,_retry-delta);
        bool cursor=pointer is {} p&&float.IsFinite(p.X)&&float.IsFinite(p.Y)&&_body.Screens.Any(s=>s.Bounds.HasPoint((Vector2I)p));
        float distance=cursor?pointer!.Value.DistanceTo(_body.Body.Foot):float.PositiveInfinity;
        bool follow=distance<=(_following?_settings.FollowExitRadius:_settings.FollowRadius);
        if(_following&&!follow){_goal=null;_wait=Pause();Machine.Request(_idle,"cursor left interest area");}
        _following=follow;
        if(follow&&_retry<=0&&_body.Body.Motion==PlatformMotion.Grounded)
        {
            var target=Project(pointer!.Value);
            if(target!=null)
            {
                if(target.SupportId==_body.Body.SupportId&&Math.Abs(target.Position.X-_body.Body.Foot.X)<=target.Tolerance)
                {_goal=null;Machine.Request(_idle,"cursor dead zone");}
                else {_goal=target;Machine.Request(_moving,"cursor interest");}
            }
        }
        else if(!follow&&_goal==null&&_wait<=0&&_body.Body.Motion==PlatformMotion.Grounded)
        {
            var surface=_body.Surfaces.FirstOrDefault(s=>s.Id==_body.Body.SupportId&&s.Contains(_body.Body.Foot.X,_body.Settings.HalfWidth));
            if(surface!=null)
            {
                float span=_settings.WanderMinDistance+(float)_random.NextDouble()*(_settings.WanderMaxDistance-_settings.WanderMinDistance);
                int direction=_random.Next(2)==0?-1:1;
                float x=Math.Clamp(_body.Body.Foot.X+direction*span,surface.Left+_body.Settings.HalfWidth,surface.Right-_body.Settings.HalfWidth);
                if(Math.Abs(x-_body.Body.Foot.X)<16)x=Math.Clamp(_body.Body.Foot.X-direction*span,surface.Left+_body.Settings.HalfWidth,surface.Right-_body.Settings.HalfWidth);
                if(Math.Abs(x-_body.Body.Foot.X)>=16){_goal=new(new(x,surface.Y),surface.Id);Machine.Request(_moving,"wander destination");}else _wait=Pause();
            }
        }
        Machine.Tick(delta);
    }
    private MovementGoal? Project(Vector2 pointer)
    {
        foreach(var surface in _body.Surfaces.Where(s=>s.Right-s.Left>=_body.Settings.HalfWidth*2).OrderBy(s=>new Vector2(Math.Clamp(pointer.X,s.Left+_body.Settings.HalfWidth,s.Right-_body.Settings.HalfWidth),s.Y).DistanceSquaredTo(pointer)))
        {
            float x=Math.Clamp(pointer.X,surface.Left+_body.Settings.HalfWidth,surface.Right-_body.Settings.HalfWidth);
            var goal=new MovementGoal(new(x,surface.Y),surface.Id,_settings.StopDistance);
            // Following on the same support needs no graph search every frame.
            if(surface.Id==_body.Body.SupportId||Movement.CanReach(goal))return goal;
        }
        return null;
    }
}
