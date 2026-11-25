using Godot;
using SiteDesktop.Behavior.Zombies;
using SiteDesktop.Desktop;
using SiteDesktop.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Simulation;

public enum DummyKind { Temporary, Trash, Zombie }
public enum DummyRemovalReason { Collected, Expired, Broken }
public sealed record DummyRemoval(int Id,DummyKind Kind,string? AppearanceId,Vector2 Foot,bool Flipped,int Frame,DummyRemovalReason Reason,double Clock);
public sealed class DummyObject
{
    public int Id { get; }
    public DummyKind Kind { get; internal set; }
    public PlatformWorld Physics { get; }
    public Vector2 Foot => Physics.Body.Foot;
    public double Age { get; internal set; }
    public string? ClaimedBy { get; internal set; }
    public string? OwnerId { get; internal set; }
    public double? MutationAge { get; internal set; }
    public string? AppearanceId { get; internal set; }
    public double? BrokenAge { get; internal set; }
    public ZombieAnimation? ZombieAnimation { get; internal set; }
    internal ZombieBehavior? Behavior;
    internal DummyObject(int id,DummyKind kind,PlatformWorld physics){Id=id;Kind=kind;Physics=physics;}
}

// Owns shared desktop objects independently of any SCP controller.
public sealed class DesktopObjectState
{
    public DesktopObjectSettings Settings { get; }
    public IReadOnlyList<DesktopScreen> Screens { get; private set; }
    public bool Visible { get; private set; }=true;
    public bool Stopped { get; private set; }
    public bool ZombiesActive { get; private set; }=true;
    public double Clock { get; private set; }
    public double ZombieClock { get; private set; }
    public int? DraggedObject { get; private set; }
    public bool AutoSpawn { get; set; }=true;
    public Func<string?>? ChooseAppearance { get; set; }
    public Func<string,bool>? OwnerActive { get; set; }
    public int Collected { get; private set; }
    public int Expired { get; private set; }
    public int Broken { get; private set; }
    public int Transformations { get; private set; }
    public IReadOnlyDictionary<int,DummyObject> Objects=>_objects;
    public event Action<DummyRemoval>? Removed;
    public event Action? PresentationChanged;
    public event Action<int>? TargetInvalidated;
    private readonly Dictionary<int,DummyObject> _objects=new();
    public IReadOnlyList<WindowObservation> Observations=>_observations;
    private IReadOnlyList<WindowObservation> _observations=Array.Empty<WindowObservation>();
    public Func<SpawnAnchor?>? SpawnAnchorProvider { get; set; }
    private readonly ObjectSpawnPolicy _spawnPolicy=new();
    private int _nextId;
    private double _spawnAge;
    public DesktopObjectState(IReadOnlyList<DesktopScreen> screens,DesktopObjectSettings? settings=null)
    {
        Screens=screens;Settings=settings??new();
        if(Settings.SpawnInterval<=0||Settings.DataLifetime<=0||Settings.Zombies.Lifetime<=0||Settings.MaxData<0||Settings.Zombies.Maximum<0)throw new ArgumentOutOfRangeException(nameof(settings));
    }
    private DesktopScreen Nearest(Vector2 foot)=>Screens.OrderBy(s=>((Vector2)s.Usable.GetCenter()).DistanceSquaredTo(foot)).First();
    public bool IsActive(DummyObject item)=>Visible&&!Stopped&&(item.Kind!=DummyKind.Zombie||ZombiesActive);
    public int? Spawn()
    {
        if(Stopped||!Visible)return null;
        var selected=SpawnAnchorProvider==null?Screens[0]:_spawnPolicy.Select(Screens,SpawnAnchorProvider());
        if(selected is not {} screen)return null;var area=screen.Usable;
        if(area.Size.X<80||area.Size.Y<64)return null;
        float margin=Math.Min(140,area.Size.X/2f);float span=Math.Max(1,area.Size.X-margin*2);
        return SpawnAt(new(area.Position.X+margin+((510+_nextId*157)%span),area.End.Y-8),_nextId%2==0?DummyKind.Temporary:DummyKind.Trash);
    }
    public int? SpawnAt(Vector2 foot,DummyKind kind=DummyKind.Temporary)
    {
        if(Stopped||!Visible||!float.IsFinite(foot.X)||!float.IsFinite(foot.Y)||kind==DummyKind.Zombie)return null;
        if(_objects.Values.Count(o=>o.Kind!=DummyKind.Zombie)>=Settings.MaxData)return null;
        string? appearance=ChooseAppearance?.Invoke();
        if(ChooseAppearance!=null&&appearance==null)return null;
        var world=new PlatformWorld(Screens,new PlatformSettings{Speed=Settings.Zombies.Speed,HalfWidth=28,Height=56});
        world.Observe(_observations);world.Place(foot);world.Body.AutoJump=false;
        var item=new DummyObject(++_nextId,kind,world){AppearanceId=appearance};_objects.Add(item.Id,item);return item.Id;
    }
    public bool TryClaim(int id,string owner)
    {
        if(Stopped||!Visible||OwnerActive?.Invoke(owner)==false||string.IsNullOrWhiteSpace(owner)||!_objects.TryGetValue(id,out var item)||item.Kind==DummyKind.Zombie||item.BrokenAge!=null||item.Physics.Body.Motion==PlatformMotion.Dragging)return false;
        if(item.ClaimedBy!=null&&item.ClaimedBy!=owner)return false;
        item.ClaimedBy=owner;return true;
    }
    public bool Release(int id,string owner)
    {
        if(!_objects.TryGetValue(id,out var item)||item.ClaimedBy!=owner)return false;
        item.ClaimedBy=null;return true;
    }
    public bool Break(int id,string owner)
    {
        if(Stopped||!Visible||OwnerActive?.Invoke(owner)==false||string.IsNullOrWhiteSpace(owner)||!_objects.TryGetValue(id,out var item)||item.Kind==DummyKind.Zombie||item.BrokenAge!=null||item.ClaimedBy!=owner)return false;
        item.BrokenAge=0;item.ClaimedBy=null;Broken++;TargetInvalidated?.Invoke(id);return true;
    }
    public bool Collect(int id)
    {
        if(Stopped||!Visible||!_objects.TryGetValue(id,out var item)||!IsActive(item))return false;
        if(item.Kind==DummyKind.Zombie)Collected++;
        Remove(id,DummyRemovalReason.Collected);return true;
    }
    private void Remove(int id,DummyRemovalReason reason)
    {
        TargetInvalidated?.Invoke(id);
        if(DraggedObject==id)DraggedObject=null;
        if(_objects.Remove(id,out var item))
        {
            var removal=new DummyRemoval(id,item.Kind,item.AppearanceId,item.Foot,item.ZombieAnimation?.Flipped??false,item.ZombieAnimation?.Frame??0,reason,ZombieClock);
            item.Physics.Stop();Removed?.Invoke(removal);
        }
    }
    public bool Transform(int id,string owner)
    {
        if(!Visible||Stopped||!ZombiesActive||OwnerActive?.Invoke(owner)==false||!_objects.TryGetValue(id,out var item)||item.Kind==DummyKind.Zombie||item.BrokenAge!=null||item.ClaimedBy!=owner||_objects.Values.Count(o=>o.Kind==DummyKind.Zombie)>=Settings.Zombies.Maximum)return false;
        item.Kind=DummyKind.Zombie;item.OwnerId=owner;item.Age=0;item.MutationAge=0;item.ClaimedBy=null;
        item.Behavior=new(item.Physics);Transformations++;PresentationChanged?.Invoke();return true;
    }
    public bool BeginObjectDrag(int id,Vector2 pointer)
    {
        if(!_objects.TryGetValue(id,out var item)||!IsActive(item))return false;
        EndDrag();TargetInvalidated?.Invoke(id);item.ClaimedBy=null;DraggedObject=id;item.Physics.Walk(0);item.Physics.BeginDrag(pointer);PresentationChanged?.Invoke();return true;
    }
    public void Drag(Vector2 pointer){if(DraggedObject is int id&&_objects.TryGetValue(id,out var item)&&IsActive(item))item.Physics.Drag(pointer);}
    public void EndDrag(){if(DraggedObject is int id&&_objects.TryGetValue(id,out var item))item.Physics.EndDrag();DraggedObject=null;PresentationChanged?.Invoke();}
    public void ReleaseOwner(string owner){foreach(var item in _objects.Values)if(item.ClaimedBy==owner)item.ClaimedBy=null;}
    public void Observe(IReadOnlyList<WindowObservation> windows)
    {
        if(Stopped)return;_observations=windows;
        foreach(var item in _objects.Values)if(IsActive(item))item.Physics.Observe(windows);
    }
    public void Tick(double seconds)
    {
        if(!double.IsFinite(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
        if(Stopped||!Visible)return;double dt=Math.Min(.1,seconds);Clock+=dt;if(ZombiesActive)ZombieClock+=dt;_spawnAge+=dt;
        if(AutoSpawn&&_spawnAge>=Settings.SpawnInterval){_spawnAge=0;Spawn();}
        foreach(var item in _objects.Values.ToArray())
        {
            if(!IsActive(item))continue;
            item.Age+=dt;
            if(item.BrokenAge is double broken){item.BrokenAge=broken+dt;if(item.BrokenAge>=.8){Remove(item.Id,DummyRemovalReason.Broken);continue;}}
            if(item.MutationAge is double age)item.MutationAge=age+dt<Settings.Zombies.MutationSeconds?age+dt:null;
            if(item.Age>=(item.Kind==DummyKind.Zombie?Settings.Zombies.Lifetime:Settings.DataLifetime)){Expired++;Remove(item.Id,DummyRemovalReason.Expired);continue;}
            if(item.Kind==DummyKind.Zombie)item.Behavior?.Tick(dt);else item.Physics.Walk(0);
            item.Physics.Tick(dt);
        }
        PresentationChanged?.Invoke();
    }
    public void ReplaceScreens(IReadOnlyList<DesktopScreen> screens)
    {
        if(Stopped||screens.Count==0)return;EndDrag();Screens=screens;_observations=Array.Empty<WindowObservation>();
        foreach(var item in _objects.Values)item.Physics.ReplaceScreens(screens);
    }
    public void SetActivity(bool active,bool zombiesActive,bool clearEffects=false)
    {
        if(Stopped)return;
        if(Visible!=active||ZombiesActive!=zombiesActive)EndDrag();
        Visible=active;ZombiesActive=zombiesActive;
        foreach(var item in _objects.Values){item.Physics.SetVisible(IsActive(item));if(clearEffects)item.MutationAge=null;}
    }
    public void Shutdown(){if(Stopped)return;EndDrag();Stopped=true;Visible=false;foreach(var item in _objects.Values)item.Physics.Stop();_objects.Clear();}
}
