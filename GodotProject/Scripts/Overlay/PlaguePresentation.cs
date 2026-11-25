using Godot;
using SiteDesktop.Companions;
using SiteDesktop.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Overlay;

public sealed record PlagueDisappearance(DummyRemoval Snapshot,string Tag)
{
    public double Age(double clock)=>Math.Max(0,clock-Snapshot.Clock);
    public int Frame(double clock)=>ZombieAnimation.Set.FrameAt(Tag,Age(clock));
}
public sealed record PlagueAura(int Id,Vector2 Foot,string Tag,double Age,bool Dragged);

// Owned by DesktopObjectController. No gameplay identity or input remains in a disappearance.
public sealed class PlaguePresentation : IDisposable
{
    public const string EffectPath="res://Assets/Sprites/vfx/Scp049Mutation/";
    public static SpriteAnimationSet EffectSet { get; }=new(EffectPath+"Scp049Mutation.contract.json");
    private readonly DesktopObjectState _state;
    public Func<TreatmentPresentationState?>? Treatment { get; set; }
    private readonly Dictionary<int,PlagueDisappearance> _disappearing=new();
    public IReadOnlyDictionary<int,PlagueDisappearance> Disappearing=>_disappearing;
    public PlaguePresentation(DesktopObjectState state){_state=state;state.Removed+=Removed;state.PresentationChanged+=SyncAnimations;SyncAnimations();}
    private void SyncAnimations()
    {
        foreach(var item in _state.Objects.Values)
            if(item.Kind==DummyKind.Zombie&&_state.IsActive(item))
            {item.ZombieAnimation??=new(item,_state.ZombieClock);item.ZombieAnimation.Update(item,_state.ZombieClock);}
    }
    private void Removed(DummyRemoval removal)
    {
        if(removal.Kind==DummyKind.Zombie&&removal.Reason is DummyRemovalReason.Collected or DummyRemovalReason.Expired)
            _disappearing.TryAdd(removal.Id,new(removal,removal.Reason==DummyRemovalReason.Collected?"collect":"expire"));
    }
    public void Sync()
    {
        if(_state.Stopped){_disappearing.Clear();return;}
        foreach(var pair in _disappearing.ToArray())if(pair.Value.Age(_state.ZombieClock)+1e-9>=ZombieAnimation.Set.Duration(pair.Value.Tag))_disappearing.Remove(pair.Key);
    }
    public IEnumerable<string?> RetainedAppearances=>_state.Objects.Values.Select(o=>o.AppearanceId).Concat(_disappearing.Values.Select(d=>d.Snapshot.AppearanceId));
    public IEnumerable<PlagueAura> Auras()
    {
        foreach(var item in _state.Objects.Values)
        {
            if(!_state.ZombiesActive)continue;
            // The legacy normal-data foot is its body bottom. The art pivot includes 8px for feet.
            if(item.MutationAge is double age&&age<EffectSet.Duration("transform"))yield return new(item.Id,item.Foot,"transform",age,_state.DraggedObject==item.Id);
            else if(Treatment?.Invoke() is {Visible:true} treatment&&treatment.TargetId==item.Id)
                yield return new(item.Id,item.Foot+new Vector2(0,8),"treat",treatment.Age,false);
        }
    }
    public void ReplaceScreens()
    {
        // Removed objects cannot be reattached by physics; discard their old-monitor snapshots.
        _disappearing.Clear();
    }
    public void Dispose(){_state.Removed-=Removed;_state.PresentationChanged-=SyncAnimations;_disappearing.Clear();}
}
