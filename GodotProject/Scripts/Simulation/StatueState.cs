using Godot;
using SiteDesktop.Behavior.Scp173;
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
    public StatueBehavior Behavior { get; }
    public bool Observed=>Behavior.Observed;
    public bool Dragging=>Physics.Body.Motion==PlatformMotion.Dragging;
    public double UnseenSeconds=>Behavior.UnseenSeconds;
    public int Attacks=>Behavior.Attacks;
    public double AttackAge=>Behavior.AttackAge;
    public Vector2 Position=>Physics.Body.Foot-new Vector2(32,88);
    private readonly StressState _shared;

    public StatueState(IReadOnlyList<DesktopScreen> screens,StressState shared)
    {
        _shared=shared;Physics=new(screens,new PlatformSettings{HalfWidth=26,Height=88});
        Stress=new(Id,Vector2.Zero,40);shared.Add(Stress);Behavior=new(Physics,Stress,Id);Reset();
    }
    public void Reset()
    {
        if(Stopped)return;var area=Physics.Screens[0].Usable;Physics.Place(new(area.End.X-360,area.End.Y-8));
        for(int i=0;i<3;i++)Physics.Tick(.1);Behavior.Reset();Stress.Position=Physics.Body.Foot;
    }
    public void Tick(double seconds,Vector2? cursor,IReadOnlyList<DesktopScreen> desktop,DesktopObjectState targets)
    {
        if(!double.IsFinite(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
        if(Stopped||!Visible)return;double dt=Math.Min(.1,seconds);
        Physics.Walk(0);Physics.Tick(dt);Behavior.Tick(dt,cursor,desktop,targets);Stress.Position=Physics.Body.Foot;
    }
    public void BeginDrag(Vector2 pointer){if(Visible&&!Stopped){Physics.BeginDrag(pointer);Behavior.Interrupt();}}
    public void Drag(Vector2 pointer){if(Visible&&!Stopped&&Dragging){Physics.Drag(pointer);Stress.Position=Physics.Body.Foot;}}
    public void EndDrag(){if(Dragging)Physics.EndDrag();}
    public void SetVisible(bool visible){if(Stopped)return;EndDrag();Visible=visible;Stress.Active=visible;Physics.SetVisible(visible);Behavior.Interrupt();}
    public void Shutdown(){if(Stopped)return;Stopped=true;Visible=false;Physics.Stop();_shared.Remove(Id);}
}
