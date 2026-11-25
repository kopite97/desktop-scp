using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Simulation;
using SiteDesktop.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Companions;

public enum CompanionMotion { Grounded, Takeoff, Falling, Landing, Balancing }
public enum CompanionGesture { None, Pet, Drag }
public enum CompanionEffectKind { Reaction, Trail }
public sealed class CompanionEffect
{
    public int Id { get; init; }
    public CompanionEffectKind Kind { get; init; }
    public string Tag { get; init; }="";
    public Vector2 Origin { get; internal set; }
    public string? SupportId { get; init; }
    public Vector2 SupportOrigin { get; internal set; }
    public double Age { get; internal set; }
    public double Lifetime { get; init; }
    public bool Flipped { get; init; }
    public Vector2 Position => Origin+(Kind==CompanionEffectKind.Reaction?new Vector2(0,(float)(-36*Age/Lifetime)):Vector2.Zero);
}

// The single owner of behavior, gestures, time and VFX. Native input and tests call these same methods.
public sealed class Scp999State
{
    public static readonly Vector2I Size=new(128,128);
    public PlatformWorld Physics { get; }
    public Vector2 Position => Foot-new Vector2(64,112);
    public Vector2 Foot => Physics.Body.Foot;
    public bool Visible { get; private set; }=true;
    public bool Stopped { get; private set; }
    public bool Flipped { get; private set; }
    public bool Overheated { get; set; }
    public string Tag { get; private set; }="idle";
    public double TagAge { get; private set; }
    public int Frame => Body.FrameAt(Tag,TagAge);
    public CompanionGesture Gesture { get; private set; }
    public CompanionMotion Motion { get; private set; }
    public Scp999Settings Settings { get; }
    public StressState Stress { get; }
    public SpriteAnimationSet Body { get; }
    public SpriteAnimationSet Reactions { get; }
    public SpriteAnimationSet Trails { get; }
    public IReadOnlyList<DesktopScreen> Screens => Physics.Screens;
    private readonly List<CompanionEffect> _effects=new();
    public IReadOnlyList<CompanionEffect> Effects => _effects;
    public int PetPresses { get; private set; }
    public int TypingReactions { get; private set; }
    private Vector2 _lastPointer;
    private double _reactionWait,_trailWait;
    private float _trailDistance;
    private int _nextEffect;

    public Scp999State(IReadOnlyList<DesktopScreen> screens,SpriteAnimationSet body,SpriteAnimationSet reactions,SpriteAnimationSet trails,StressState stress,Scp999Settings? settings=null)
    {
        if(screens.Count==0)throw new ArgumentException("A desktop work area is required.");
        Body=body;Reactions=reactions;Trails=trails;Stress=stress;Settings=settings??new();
        Physics=new(screens,new PlatformSettings{Speed=Settings.Speed,HalfWidth=64,Height=112});ResetPosition();
    }
    private void SetTag(string tag) { if(Tag==tag)return;Tag=tag;TagAge=0; }
    public void ResetPosition()
    {
        if(Stopped)return;Gesture=CompanionGesture.None;Motion=CompanionMotion.Grounded;_effects.Clear();
        Physics.Walk(0);Physics.Place(new Vector2(Screens[0].Usable.Position.X+304,Screens[0].Usable.End.Y-8));
        for(int i=0;i<3;i++)Physics.Tick(.1);
        Tag="idle";TagAge=0;_trailDistance=0;_trailWait=0;_reactionWait=0;
    }
    public void ReplaceScreens(IReadOnlyList<DesktopScreen> screens)
    {
        if(Stopped||screens.Count==0)return;Physics.ReplaceScreens(screens);Gesture=CompanionGesture.None;
        _effects.Clear();_trailDistance=0;SyncMotion(true);
    }
    public void ObserveWindows(IReadOnlyList<WindowObservation> windows)
    {
        if(Stopped||!Visible)return;Physics.Observe(windows);
        foreach(var e in _effects.Where(e=>e.Kind==CompanionEffectKind.Trail).ToArray())
        {
            var next=Physics.Surfaces.FirstOrDefault(p=>p.Id==e.SupportId&&p.Contains(e.Origin.X+p.WindowOrigin.X-e.SupportOrigin.X,32));
            if(next==null||(next.WindowOrigin-e.SupportOrigin).Length()>Physics.Settings.MaxCarryDelta){_effects.Remove(e);continue;}
            e.Origin+=next.WindowOrigin-e.SupportOrigin;e.SupportOrigin=next.WindowOrigin;
        }
        SyncMotion();
    }
    public bool BeginPet(Vector2 pointer)
    {
        if(Stopped||!Visible||Gesture!=CompanionGesture.None||Motion!=CompanionMotion.Grounded||Physics.Body.SupportId==null)return false;
        Physics.Walk(0);Gesture=CompanionGesture.Pet;_lastPointer=pointer;PetPresses++;SetTag("pet");
        Stress.Relieve(Foot,Settings.PetRadius,Settings.ClickRelief);return true;
    }
    public bool BeginDrag(Vector2 pointer)
    {
        if(Stopped||!Visible||Gesture==CompanionGesture.Drag)return false;
        Physics.Walk(0);Physics.BeginDrag(pointer);Gesture=CompanionGesture.Drag;SetTag("idle");return true;
    }
    public void EndGesture()
    {
        if(Gesture==CompanionGesture.Drag){Physics.EndDrag();Gesture=CompanionGesture.None;SyncMotion(true);}
        else if(Gesture==CompanionGesture.Pet)SetTag("pet_recover");
        Gesture=CompanionGesture.None;_trailDistance=0;
    }
    private void SyncMotion(bool force=false)
    {
        if(Gesture==CompanionGesture.Drag)return;
        var motion=Physics.Body.Motion switch {PlatformMotion.Takeoff=>CompanionMotion.Takeoff,PlatformMotion.Falling=>CompanionMotion.Falling,PlatformMotion.Landing=>CompanionMotion.Landing,PlatformMotion.Balancing=>CompanionMotion.Balancing,_=>CompanionMotion.Grounded};
        if(motion==CompanionMotion.Grounded&&Tag=="land"&&TagAge<Body.Duration("land"))return;
        if(!force&&motion==Motion)return;
        var previous=Motion;Motion=motion;
        if(motion!=CompanionMotion.Grounded)Gesture=CompanionGesture.None;
        SetTag(motion switch {CompanionMotion.Takeoff=>"takeoff",CompanionMotion.Falling=>"fall",CompanionMotion.Landing=>"land",CompanionMotion.Balancing=>"balance",_=>previous==CompanionMotion.Balancing?"balance_recover":"idle"});
        _trailDistance=0;
    }
    public void TypingActivity()
    {
        if(Stopped||!Visible||Gesture!=CompanionGesture.None||Motion!=CompanionMotion.Grounded||Physics.Body.SupportId==null||_reactionWait>0)return;
        if(Tag=="pet_recover"||Tag=="land"||Tag=="balance_recover")return;
        _reactionWait=Settings.ReactionCooldown;TypingReactions++;
        // Do not restart an in-flight reaction animation on keyboard auto-repeat.
        SetTag("typing_react");
        var a=Body.Frames[Frame].ReactionAnchor;float x=Flipped?64-a.X:a.X;
        string tag=TypingReactions%2==1?"heart":"note";
        AddEffect(new CompanionEffect {Id=++_nextEffect,Kind=CompanionEffectKind.Reaction,Tag=tag,Origin=Position+new Vector2(x*2+(tag=="heart"?-28:28),a.Y*2-20),Lifetime=Reactions.Duration(tag)},Settings.MaxReactions);
    }
    private void AddEffect(CompanionEffect effect,int max)
    {
        if(max<=0)return;
        while(_effects.Count(e=>e.Kind==effect.Kind)>=max)_effects.Remove(_effects.First(e=>e.Kind==effect.Kind));
        _effects.Add(effect);
    }
    public void Tick(double delta,Vector2 pointer,bool leftDown=false,bool rightDown=false,bool overBody=false)
    {
        if(!double.IsFinite(delta)||delta<0)throw new ArgumentOutOfRangeException(nameof(delta));
        if(Stopped||!Visible)return;
        // Bound pause/resume stalls. Tests and live rendering share this elapsed-time policy.
        delta=Math.Min(delta,.1);TagAge+=delta;_reactionWait=Math.Max(0,_reactionWait-delta);_trailWait+=delta;
        foreach(var effect in _effects)effect.Age+=delta;
        _effects.RemoveAll(e=>e.Age>=e.Lifetime);
        Physics.Walk(0);
        if(Gesture==CompanionGesture.Drag)
        {
            Physics.Drag(pointer);if(!rightDown)EndGesture();Physics.Tick(delta);SyncMotion();return;
        }
        SyncMotion();
        if(Gesture==CompanionGesture.Pet)
        {
            if(!leftDown)EndGesture();
            else if(overBody&&pointer.DistanceSquaredTo(_lastPointer)>=4)Stress.Relieve(Foot,Settings.PetRadius,Settings.RubReliefPerSecond*(float)delta);
            _lastPointer=pointer;Physics.Tick(delta);SyncMotion();return;
        }
        bool ready=Body.Clips[Tag].Loop||TagAge>=Body.Duration(Tag);
        float dx=pointer.X-Foot.X;
        if(Motion==CompanionMotion.Grounded&&ready&&Screens.Any(s=>s.Bounds.HasPoint((Vector2I)pointer))&&pointer.DistanceTo(Foot)<=Settings.FollowRadius&&Math.Abs(dx)>Settings.StopDistance)
        {Flipped=dx<0;Physics.Walk(Math.Sign(dx));}
        var old=Foot;Physics.Tick(delta);SyncMotion();
        if(Motion!=CompanionMotion.Grounded||(!Body.Clips[Tag].Loop&&TagAge<Body.Duration(Tag)))return;
        float travelled=Foot.DistanceTo(old);SetTag(Physics.Body.WalkDirection!=0&&travelled>0?"move":"idle");_trailDistance+=travelled;
        if(Physics.Body.WalkDirection!=0&&travelled>0&&Physics.Body.SupportId!=null&&_trailWait>=Settings.TrailInterval&&_trailDistance>=Settings.TrailSpacing)
        {
            string tag=_nextEffect%2==0?"puddle_a":"puddle_b";
            AddEffect(new CompanionEffect {Id=++_nextEffect,Kind=CompanionEffectKind.Trail,Tag=tag,Origin=Foot,SupportId=Physics.Body.SupportId,SupportOrigin=Physics.Body.SupportOrigin,Lifetime=Trails.Duration(tag)-.2+Settings.TrailHoldSeconds,Flipped=Flipped},Settings.MaxTrails);
            _trailDistance=0;_trailWait=0;
        }
    }
    public int EffectFrame(CompanionEffect effect) => effect.Kind==CompanionEffectKind.Reaction?Reactions.FrameAt(effect.Tag,effect.Age):Trails.FrameAt(effect.Tag,effect.Age,Settings.TrailHoldSeconds);
    public void SetVisible(bool visible)
    {
        if(Stopped)return;
        if(!visible&&Gesture==CompanionGesture.Drag)EndGesture();
        Physics.SetVisible(visible);Visible=visible;Gesture=CompanionGesture.None;_effects.Clear();_reactionWait=0;_trailDistance=0;_trailWait=0;Tag="idle";TagAge=0;if(visible)SyncMotion(true);
    }
    public void Shutdown(){if(Stopped)return;SetVisible(false);Physics.Stop();Stopped=true;}
}
