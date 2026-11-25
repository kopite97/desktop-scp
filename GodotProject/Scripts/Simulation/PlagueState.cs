using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Simulation;

// SCP-049 behavior. Shared objects have a separate owner; facade methods preserve diagnostic commands.
public sealed class PlagueState
{
    public const string DoctorId="scp049";
    public PlatformWorld Doctor { get; }
    public DoctorAnimation Animation { get; }=new();
    public StressTarget DoctorStress { get; }
    public PlagueSettings Settings { get; }
    public DoctorPhase Phase { get; private set; }
    public int? TargetId { get; private set; }
    public int? DraggedObject => World.DraggedObject;
    public double TreatmentAge { get; private set; }
    public double? TreatmentStartedAt { get; private set; }
    public event Action<DummyRemoval>? Removed { add=>World.Removed+=value; remove=>World.Removed-=value; }
    public float TreatmentRate { get; set; }=1;
    public double Clock { get; private set; }
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
    private readonly Dictionary<int,double> _retry=new();
    private IReadOnlyList<WindowObservation> _observations=Array.Empty<WindowObservation>();
    private readonly StressState _stress;

    private double _seekAge;

    public PlagueState(IReadOnlyList<DesktopScreen> screens,StressState stress,PlagueSettings? settings=null,DesktopObjectState? world=null)
    {
        _ownsWorld=world==null;World=world??new(screens,settings);Settings=World.Settings;_stress=stress;World.TargetInvalidated+=Invalidated;
        if(Settings.SpawnInterval<=0||Settings.TreatmentSeconds<=0||Settings.DataLifetime<=0||Settings.ZombieLifetime<=0||Settings.MaxData<0||Settings.MaxZombies<0)throw new ArgumentOutOfRangeException(nameof(settings));
        Doctor=new(screens,new PlatformSettings{Speed=Settings.DoctorSpeed,HalfWidth=40,Height=112});
        DoctorStress=new(DoctorId,Doctor.Body.Foot,40);stress.Add(DoctorStress);ResetDoctor();
    }
    private DesktopScreen Nearest(Vector2 foot)=>Screens.OrderBy(s=>((Vector2)s.Usable.GetCenter()).DistanceSquaredTo(foot)).First();
    private static bool Settled(PlatformWorld world)=>world.Body.SupportId!=null&&world.Body.Motion==PlatformMotion.Grounded;
    public int? Spawn()=>World.Spawn();

    public int? SpawnAt(Vector2 foot,DummyKind kind=DummyKind.Temporary)=>World.SpawnAt(foot,kind);

    public bool TryClaim(int id,string owner)=>World.TryClaim(id,owner);

    public bool Release(int id,string owner)=>World.Release(id,owner);

    public bool Break(int id,string owner)=>World.Break(id,owner);

    private void CancelTarget(bool retry=false)
    {
        if(TargetId is int id){Release(id,DoctorId);if(retry)_retry[id]=Clock+Settings.RetrySeconds;}
        TargetId=null;TreatmentAge=0;TreatmentStartedAt=null;_seekAge=0;
        if(Phase!=DoctorPhase.Dragging)Phase=DoctorPhase.Idle;
        Doctor.Walk(0);
        Animation.Update(this,0);
    }
    public bool Collect(int id)=>World.Collect(id);

    private void Invalidated(int id){if(TargetId==id)CancelTarget();_retry.Remove(id);}

    private bool InTreatmentRange(DummyObject item)=>Settled(Doctor)&&Settled(item.Physics)&&Doctor.Body.SupportId==item.Physics.Body.SupportId&&Math.Abs(Doctor.Body.Foot.X-item.Foot.X)<=Settings.TreatmentRange&&Math.Abs(Doctor.Body.Foot.Y-item.Foot.Y)<1;
    public bool CompleteTreatment(int id,string owner)
    {
        if(Stopped||!Visible||owner!=DoctorId||TargetId!=id||Phase!=DoctorPhase.Treating||TreatmentAge+1e-8<Settings.TreatmentSeconds||!_objects.TryGetValue(id,out var item))return false;
        if(item.Kind==DummyKind.Zombie||item.ClaimedBy!=owner||!InTreatmentRange(item)||_objects.Values.Count(o=>o.Kind==DummyKind.Zombie)>=Settings.MaxZombies)return false;
        if(!World.Transform(id,owner))return false;CancelTarget();Animation.Update(this,0);return true;
    }
    public bool BeginDoctorDrag(Vector2 pointer)
    {
        if(Stopped||!Visible)return false;EndDrag();CancelTarget();Phase=DoctorPhase.Dragging;Doctor.BeginDrag(pointer);Animation.Update(this,0);return true;
    }
    public bool BeginObjectDrag(int id,Vector2 pointer){EndDrag();return World.BeginObjectDrag(id,pointer);}

    public void Drag(Vector2 pointer)
    {
        if(Stopped||!Visible)return;
        if(Phase==DoctorPhase.Dragging)Doctor.Drag(pointer);
        else World.Drag(pointer);
        DoctorStress.Position=Doctor.Body.Foot;
        Animation.Update(this,0);
    }
    public void EndDrag()
    {
        if(Phase==DoctorPhase.Dragging){Doctor.EndDrag();Phase=DoctorPhase.Idle;}
        World.EndDrag();
        Animation.Update(this,0);
    }
    public void Observe(IReadOnlyList<WindowObservation> windows){World.Observe(windows);ObserveDoctor(windows);}
    public void ObserveDoctor(IReadOnlyList<WindowObservation> windows)
    {
        if(Stopped||!Visible)return;_observations=windows;Doctor.Observe(windows);
        if(Phase==DoctorPhase.Treating&&TargetId is int id&&(!_objects.TryGetValue(id,out var item)||!InTreatmentRange(item)))CancelTarget();
        DoctorStress.Position=Doctor.Body.Foot;
        Animation.Update(this,0);
    }


    public void Tick(double seconds){World.Tick(seconds);TickDoctor(seconds);}
    public void TickDoctor(double seconds)
    {
        TickCore(seconds);
        Animation.Update(this,Visible&&!Stopped?Math.Clamp(seconds,0,.1):0);
    }

    private void TickCore(double seconds)
    {
        if(!double.IsFinite(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
        if(Stopped||!Visible)return;double dt=Math.Min(.1,seconds);Clock+=dt;
        if(Phase==DoctorPhase.Dragging){DoctorStress.Position=Doctor.Body.Foot;return;}
        Doctor.Walk(0);
        if(TargetId is int current&&(!_objects.TryGetValue(current,out var held)||held.Kind==DummyKind.Zombie||held.ClaimedBy!=DoctorId))CancelTarget();
        if(Phase==DoctorPhase.Treating&&TargetId is int treating&&!InTreatmentRange(_objects[treating]))CancelTarget();
        if(TargetId==null&&Settled(Doctor)&&_objects.Values.Count(o=>o.Kind==DummyKind.Zombie)<Settings.MaxZombies)
        {
            int screen=Nearest(Doctor.Body.Foot).Index;
            var target=_objects.Values.Where(o=>o.Kind!=DummyKind.Zombie&&o.BrokenAge==null&&o.ClaimedBy==null&&o.Physics.Body.Motion!=PlatformMotion.Dragging&&Nearest(o.Foot).Index==screen&&(!_retry.TryGetValue(o.Id,out var retry)||Clock>=retry))
                .OrderBy(o=>o.Foot.DistanceSquaredTo(Doctor.Body.Foot)).ThenBy(o=>o.Id).FirstOrDefault();
            if(target!=null&&TryClaim(target.Id,DoctorId)){TargetId=target.Id;Phase=DoctorPhase.Seeking;_seekAge=0;}
        }
        if(TargetId is int targetId)
        {
            var target=_objects[targetId];
            if(InTreatmentRange(target)){if(Phase!=DoctorPhase.Treating)TreatmentStartedAt=Clock;Phase=DoctorPhase.Treating;TreatmentAge+=dt*TreatmentRate;}
            else
            {
                Phase=DoctorPhase.Seeking;TreatmentAge=0;TreatmentStartedAt=null;_seekAge+=dt;
                if(_seekAge>=Settings.TargetTimeout)CancelTarget(true);
                else if(Settled(Doctor))
                {
                    float dx=target.Foot.X-Doctor.Body.Foot.X;int direction=Math.Sign(dx);
                    Doctor.Body.AutoJump=target.Foot.Y<=Doctor.Body.Foot.Y;
                    if(target.Foot.Y<Doctor.Body.Foot.Y-4)Doctor.JumpToAdjacent(direction);
                    if(Math.Abs(dx)<Settings.TreatmentRange&&target.Foot.Y>Doctor.Body.Foot.Y+4)
                    {
                        var support=Doctor.Surfaces.FirstOrDefault(s=>s.Id==Doctor.Body.SupportId);
                        if(support!=null&&!support.Floor)direction=Doctor.Body.Foot.X<(support.Left+support.Right)/2?-1:1;
                    }
                    Doctor.Walk(direction);
                }
            }
        }
        Doctor.Tick(dt);DoctorStress.Position=Doctor.Body.Foot;
        if(TargetId is int completed&&TreatmentAge>=Settings.TreatmentSeconds)CompleteTreatment(completed,DoctorId);
    }
    public void ResetDoctor()
    {
        if(Stopped)return;EndDrag();CancelTarget();var area=Screens[0].Usable;
        Doctor.Place(new(area.Position.X+Math.Min(420,area.Size.X-40),area.End.Y-8));
        for(int i=0;i<3;i++)Doctor.Tick(.1);DoctorStress.Position=Doctor.Body.Foot;
        Animation.Reset(this);
    }
    public void ReplaceScreens(IReadOnlyList<DesktopScreen> screens)
    {
        if(Stopped||screens.Count==0)return;EndDrag();CancelTarget();_observations=Array.Empty<WindowObservation>();Doctor.ReplaceScreens(screens);
        World.ReplaceScreens(screens);DoctorStress.Position=Doctor.Body.Foot;
    }
    public void SetVisible(bool visible){SetDoctorActive(visible,false);World.SetActivity(visible,visible);}
    public void SetDoctorActive(bool active,bool disabled)
    {
        if(Stopped)return;
        if(disabled){CancelTarget();World.ReleaseOwner(DoctorId);_retry.Clear();}
        if(Visible!=active){if(Phase==DoctorPhase.Dragging){Doctor.EndDrag();Phase=DoctorPhase.Idle;}Visible=active;Doctor.SetVisible(active);}
        DoctorStress.Active=active;
    }

    public void Shutdown()
    {
        if(Stopped)return;CancelTarget();EndDrag();Visible=false;Stopped=true;Doctor.Stop();
        World.TargetInvalidated-=Invalidated;if(_ownsWorld)World.Shutdown();_retry.Clear();_stress.Remove(DoctorId);
    }
}
