using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Simulation;
public sealed class StatueState
{
    public const string Id="scp173";
    public PlatformWorld Physics { get; }
    public StressTarget Stress { get; }
    public bool Visible { get; private set; }=true;
    public bool Stopped { get; private set; }
    public bool Observed { get; private set; }=true;
    public bool Dragging=>Physics.Body.Motion==PlatformMotion.Dragging;
    public double UnseenSeconds { get; private set; }
    public int Attacks { get; private set; }
    public double AttackAge { get; private set; }=100;
    public Vector2 Position=>Physics.Body.Foot-new Vector2(32,88);
    private readonly StressState _shared;
    private double _cooldown;
    public StatueState(IReadOnlyList<DesktopScreen> screens,StressState shared)
    {
        _shared=shared;Physics=new(screens,new PlatformSettings{HalfWidth=26,Height=88});
        Stress=new(Id,Vector2.Zero,40);shared.Add(Stress);Reset();
    }
    public void Reset()
    {
        if(Stopped)return;var area=Physics.Screens[0].Usable;Physics.Place(new(area.End.X-360,area.End.Y-8));
        for(int i=0;i<3;i++)Physics.Tick(.1);UnseenSeconds=0;_cooldown=0;Stress.Position=Physics.Body.Foot;
    }
    public void Tick(double seconds,Vector2? cursor,IReadOnlyList<DesktopScreen> desktop,PlagueState targets)=>Tick(seconds,cursor,desktop,targets.World);
    public void Tick(double seconds,Vector2? cursor,IReadOnlyList<DesktopScreen> desktop,DesktopObjectState targets)
    {
        if(!double.IsFinite(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
        if(Stopped||!Visible)return;double dt=Math.Min(.1,seconds);AttackAge+=dt;_cooldown=Math.Max(0,_cooldown-dt);
        if(Dragging){UnseenSeconds=0;Stress.Position=Physics.Body.Foot;return;}
        Physics.Walk(0);Physics.Tick(dt);Stress.Position=Physics.Body.Foot;
        DesktopScreen? here=desktop.Where(s=>s.Bounds.HasPoint((Vector2I)Physics.Body.Foot)).Select(s=>(DesktopScreen?)s).FirstOrDefault();
        DesktopScreen? pointerScreen=cursor is Vector2 pointer?desktop.Where(s=>s.Bounds.HasPoint((Vector2I)pointer)).Select(s=>(DesktopScreen?)s).FirstOrDefault():null;
        bool valid=cursor!=null&&here!=null&&pointerScreen!=null;
        Observed=!valid||cursor!.Value.DistanceTo(Position+new Vector2(32,44))<=180;
        if(Observed){UnseenSeconds=0;return;}
        UnseenSeconds+=dt;
        bool otherScreen=here!.Value.Index!=pointerScreen!.Value.Index;
        double delay=Stress.Value>=70?4:8; // Tunable prototype values recorded in PLAN-007.
        if(_cooldown>0||(!otherScreen&&UnseenSeconds<delay)||!targets.Visible||targets.Stopped)return;
        var target=targets.Objects.Values.Where(o=>o.Kind!=DummyKind.Zombie&&o.BrokenAge==null&&o.ClaimedBy==null&&o.Physics.Body.Motion==PlatformMotion.Grounded&&here.Value.Usable.HasPoint((Vector2I)o.Foot))
            .OrderBy(o=>o.Foot.DistanceSquaredTo(Physics.Body.Foot)).ThenBy(o=>o.Id).FirstOrDefault();
        _cooldown=2;
        if(target==null||!targets.TryClaim(target.Id,Id))return;
        // Recheck at the same state-owner boundary before the instantaneous transition.
        if(!targets.Break(target.Id,Id)){targets.Release(target.Id,Id);return;}
        float x=target.Foot.X+(target.Foot.X<here.Value.Usable.GetCenter().X?54:-54);
        Physics.Place(new(Math.Clamp(x,here.Value.Usable.Position.X+28,here.Value.Usable.End.X-28),target.Foot.Y));
        Attacks++;AttackAge=0;UnseenSeconds=0;Stress.Set(Stress.Value+2);Stress.Position=Physics.Body.Foot;
    }
    public void BeginDrag(Vector2 pointer){if(Visible&&!Stopped){Physics.BeginDrag(pointer);UnseenSeconds=0;}}
    public void Drag(Vector2 pointer){if(Visible&&!Stopped&&Dragging){Physics.Drag(pointer);Stress.Position=Physics.Body.Foot;}}
    public void EndDrag(){if(Dragging)Physics.EndDrag();}
    public void SetVisible(bool visible){if(Stopped)return;EndDrag();Visible=visible;Stress.Active=visible;Physics.SetVisible(visible);UnseenSeconds=0;}
    public void Shutdown(){if(Stopped)return;Stopped=true;Visible=false;Physics.Stop();_shared.Remove(Id);}
}
