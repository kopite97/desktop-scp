using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Platforms;
using SiteDesktop.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Behavior.Scp173;
public sealed class StatueBehavior
{
    private readonly PlatformWorld _body;
    private readonly StressTarget _stress;
    private readonly string _owner;
    private readonly ActionState _observed,_unseen,_attack;
    private DesktopObjectState? _targets;
    private DesktopScreen? _screen;
    private bool _otherScreen;
    private double _cooldown;
    public StateMachine Machine { get; }=new();
    public bool Observed { get; private set; }=true;
    public double UnseenSeconds { get; private set; }
    public double AttackAge { get; private set; }=100;
    public int Attacks { get; private set; }
    public StatueBehavior(PlatformWorld body,StressTarget stress,string owner)
    {
        _body=body;_stress=stress;_owner=owner;
        _observed=new("Observed",_=>UnseenSeconds=0);
        _unseen=new("Unseen",delta=>
        {
            UnseenSeconds+=delta;double delay=_stress.Value>=70?4:8;
            if(_cooldown<=0&&(_otherScreen||UnseenSeconds>=delay)&&_targets is {Visible:true,Stopped:false})Machine.Request(_attack!,"attack eligible");
        });
        _attack=new("Attack",_=>{},Attack);Reset();
    }
    public void Reset(){UnseenSeconds=0;_cooldown=0;Machine.Cancel();Machine.Request(_observed,"reset");}
    public void Interrupt(){UnseenSeconds=0;Machine.Cancel();Machine.Request(_observed,"interaction interrupted");}
    public void Tick(double delta,Vector2? cursor,IReadOnlyList<DesktopScreen> desktop,DesktopObjectState targets)
    {
        _targets=targets;AttackAge+=delta;_cooldown=Math.Max(0,_cooldown-delta);
        if(_body.Body.Motion==PlatformMotion.Dragging){Interrupt();return;}
        _screen=desktop.Where(s=>s.Bounds.HasPoint((Vector2I)_body.Body.Foot)).Select(s=>(DesktopScreen?)s).FirstOrDefault();
        var pointerScreen=cursor is {} p?desktop.Where(s=>s.Bounds.HasPoint((Vector2I)p)).Select(s=>(DesktopScreen?)s).FirstOrDefault():null;
        bool valid=cursor!=null&&_screen!=null&&pointerScreen!=null;
        Observed=!valid||cursor!.Value.DistanceTo(_body.Body.Foot-new Vector2(0,44))<=180;
        _otherScreen=valid&&_screen!.Value.Index!=pointerScreen!.Value.Index;
        Machine.Request(Observed?_observed:_unseen,Observed?"cursor observes":"cursor away");Machine.Tick(delta);
    }
    private void Attack()
    {
        if(_targets==null||_screen==null)return;
        var target=_targets.Objects.Values.Where(o=>o.Kind!=DummyKind.Zombie&&o.BrokenAge==null&&o.ClaimedBy==null&&o.Physics.Body.Motion==PlatformMotion.Grounded&&_screen.Value.Usable.HasPoint((Vector2I)o.Foot)).OrderBy(o=>o.Foot.DistanceSquaredTo(_body.Body.Foot)).ThenBy(o=>o.Id).FirstOrDefault();
        _cooldown=2;if(target==null||!_targets.TryClaim(target.Id,_owner))return;
        if(!_targets.Break(target.Id,_owner)){_targets.Release(target.Id,_owner);return;}
        var area=_screen.Value.Usable;float x=target.Foot.X+(target.Foot.X<area.GetCenter().X?54:-54);
        _body.Place(new(Math.Clamp(x,area.Position.X+28,area.End.X-28),target.Foot.Y));
        Attacks++;AttackAge=0;_stress.Set(_stress.Value+2);UnseenSeconds=0;
    }
}
