using Godot;
using SiteDesktop.Behavior.Scp049;
using SiteDesktop.Desktop;
using SiteDesktop.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Simulation;

public enum DoctorPhase { Idle, Seeking, Treating, Dragging, Transferring }

// SCP-049 composition: behavior, body, and presentation have separate owners.
public sealed class PlagueState
{
    public const string DoctorId="scp049";
    public PlatformWorld Doctor { get; }
    public TreatmentPresentationState? TreatmentView=>Phase==DoctorPhase.Treating&&TargetId is int id&&TreatmentStartedAt is double start?new(id,Math.Max(0,Clock-start),Visible&&!Stopped):null;
    public DoctorAnimation Animation { get; }=new();
    public StressTarget DoctorStress { get; }
    public PlagueSettings Settings { get; }
    public DoctorBehavior Behavior { get; }
    public DoctorPhase Phase=>Behavior.Phase;
    public int? TargetId=>Behavior.TargetId;
    public int? DraggedObject => World.DraggedObject;
    public double TreatmentAge=>Behavior.TreatmentAge;
    public double? TreatmentStartedAt=>Behavior.TreatmentStartedAt;
    public event Action<DummyRemoval>? Removed { add=>World.Removed+=value; remove=>World.Removed-=value; }
    public float TreatmentRate { get=>Behavior.TreatmentRate; set=>Behavior.TreatmentRate=value; }
    public double Clock=>Behavior.Clock;
    public bool Visible { get; private set; }=true;
    public bool Stopped { get; private set; }
    public bool AutoSpawn { get=>World.AutoSpawn; set=>World.AutoSpawn=value; }
    public Func<string?>? ChooseAppearance { get=>World.ChooseAppearance; set=>World.ChooseAppearance=value; }
    public int Transformations => World.Transformations;
    public int Collected => World.Collected;
    public int Expired => World.Expired;
    public int Broken => World.Broken;
    public IReadOnlyDictionary<int,DummyObject> Objects=>_objects;
    public IReadOnlyList<DesktopScreen> Screens=>Doctor.Screens;
    public DesktopObjectState World { get; }
    private IReadOnlyDictionary<int,DummyObject> _objects=>World.Objects;
    private readonly bool _ownsWorld;

    private IReadOnlyList<WindowObservation> _observations=Array.Empty<WindowObservation>();
    private readonly StressState _stress;



    public PlagueState(IReadOnlyList<DesktopScreen> screens,StressState stress,PlagueSettings? settings=null,DesktopObjectState? world=null,DesktopObjectSettings? objectSettings=null)
    {
        _ownsWorld=world==null;World=world??new(screens,objectSettings);Settings=settings??new();_stress=stress;World.TargetInvalidated+=Invalidated;
        if(Settings.TreatmentSeconds<=0||Settings.DoctorSpeed<=0||Settings.TargetTimeout<=0||Settings.RetrySeconds<0)throw new ArgumentOutOfRangeException(nameof(settings));
        Doctor=new(screens,new PlatformSettings{Speed=Settings.DoctorSpeed,HalfWidth=40,Height=112});
        Behavior=new(Doctor,World,Settings,DoctorId);
        DoctorStress=new(DoctorId,Doctor.Body.Foot,40);stress.Add(DoctorStress);ResetDoctor();
    }
    private DesktopScreen Nearest(Vector2 foot)=>Screens.OrderBy(s=>((Vector2)s.Usable.GetCenter()).DistanceSquaredTo(foot)).First();
    private static bool Settled(PlatformWorld world)=>world.Body.SupportId!=null&&world.Body.Motion==PlatformMotion.Grounded;
    public int? Spawn()=>World.Spawn();

    public int? SpawnAt(Vector2 foot,DummyKind kind=DummyKind.Temporary)=>World.SpawnAt(foot,kind);

    public bool TryClaim(int id,string owner)=>World.TryClaim(id,owner);

    public bool Release(int id,string owner)=>World.Release(id,owner);

    public bool Break(int id,string owner)=>World.Break(id,owner);

    private void CancelTarget(bool retry=false){Behavior.Cancel(retry);Animation.Update(this,0);}
    public bool Collect(int id)=>World.Collect(id);
    private void Invalidated(int id){Behavior.Invalidated(id);Animation.Update(this,0);}
    public bool CompleteTreatment(int id,string owner)=>Behavior.Complete(id,owner);
    public bool BeginDoctorDrag(Vector2 pointer)
    {
        if(Stopped||!Visible||Behavior.Portal.Active)return false;EndDrag();Behavior.DragStarted();Doctor.BeginDrag(pointer);Animation.Update(this,0);return true;
    }

    public void Drag(Vector2 pointer)
    {
        if(Stopped||!Visible)return;
        if(Phase==DoctorPhase.Dragging)Doctor.Drag(pointer);
        DoctorStress.Position=Doctor.Body.Foot;
        Animation.Update(this,0);
    }
    public void EndDrag()
    {
        if(Phase==DoctorPhase.Dragging){Doctor.EndDrag();Behavior.DragEnded();}
        Animation.Update(this,0);
    }
    public void ObserveDoctor(IReadOnlyList<WindowObservation> windows)
    {
        if(Stopped||!Visible)return;_observations=windows;Doctor.Observe(windows);
        Behavior.Revalidate();
        DoctorStress.Position=Doctor.Body.Foot;
        Animation.Update(this,0);
    }


    public void TickDoctor(double seconds)
    {
        TickCore(seconds);
        Animation.Update(this,Visible&&!Stopped?Math.Clamp(seconds,0,.1):0);
    }

    private void TickCore(double seconds)
    {
        if(!double.IsFinite(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
        if(Stopped||!Visible)return;double dt=Math.Min(.1,seconds);
        Behavior.Tick(dt);if(!Behavior.Portal.Active)Doctor.Tick(dt);DoctorStress.Position=Doctor.Body.Foot;
        if(TargetId is int completed&&TreatmentAge>=Settings.TreatmentSeconds)CompleteTreatment(completed,DoctorId);
    }
    public void ResetDoctor()
    {
        if(Stopped)return;EndDrag();CancelTarget();var area=Screens[0].Usable;
        Doctor.Place(new(area.Position.X+Math.Min(420,area.Size.X-40),area.End.Y-8));
        for(int i=0;i<3;i++)Doctor.Tick(.1);DoctorStress.Position=Doctor.Body.Foot;
        Animation.Reset(this);
    }
    public void ReplaceDoctorScreens(IReadOnlyList<DesktopScreen> screens)
    {
        if(Stopped||screens.Count==0)return;EndDrag();CancelTarget();_observations=Array.Empty<WindowObservation>();Doctor.ReplaceScreens(screens);
        DoctorStress.Position=Doctor.Body.Foot;
    }
    public void SetDoctorActive(bool active,bool disabled)
    {
        if(Stopped)return;
        if(disabled){CancelTarget();World.ReleaseOwner(DoctorId);Behavior.Reset();}
        if(Visible!=active){if(Phase==DoctorPhase.Dragging){Doctor.EndDrag();Behavior.DragEnded();}Visible=active;Doctor.SetVisible(active);}
        DoctorStress.Active=active;
    }

    public void Shutdown()
    {
        if(Stopped)return;CancelTarget();EndDrag();Visible=false;Stopped=true;Doctor.Stop();
        World.TargetInvalidated-=Invalidated;if(_ownsWorld)World.Shutdown();Behavior.Reset();_stress.Remove(DoctorId);
    }
}
