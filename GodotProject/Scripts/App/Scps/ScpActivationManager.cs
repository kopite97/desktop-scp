using System;
using System.Collections.Generic;
using System.Linq;
using SiteDesktop.Simulation;

namespace SiteDesktop.App.Scps;

public sealed class ScpActivationManager
{
    public sealed class Entry
    {
        public ScpDefinition Definition { get; }
        public IScpModule Module { get; }
        public bool Enabled { get; internal set; }
        public bool Active { get; internal set; }
        public string State=>!Enabled?"비활성":Active?"활성":"봉쇄 중";
        internal Entry(ScpDefinition definition,IScpModule module,bool enabled)
        {Definition=definition;Module=module;Enabled=Active=enabled;}
    }
    private readonly Dictionary<string,Entry> _entries=new();
    private readonly Func<bool> _locked;
    private bool _stopped;
    public IReadOnlyCollection<Entry> Entries=>_entries.Values;
    // null denotes a global activity change, a setting ID denotes a user preference change.
    public event Action<string?>? Changed;
    public ScpActivationManager(IEnumerable<ScpDefinition> definitions,IReadOnlyDictionary<string,bool> settings,Func<bool> locked)
    {
        _locked=locked;var list=definitions.ToArray();
        if(list.Select(d=>d.Id).Distinct().Count()!=list.Length)throw new ArgumentException("Duplicate SCP ID");
        foreach(var d in list){bool enabled=settings.TryGetValue(d.Id,out var v)?v:d.DefaultEnabled;var entry=new Entry(d,d.Create(enabled&&!locked()),enabled){Active=enabled&&!locked()};_entries.Add(d.Id,entry);}
    }
    public bool IsActive(string id)=>!_stopped&&_entries.TryGetValue(id,out var e)&&e.Active;
    public bool IsOwnerActive(string owner)=>!_stopped&&_entries.Values.Any(e=>e.Definition.RuntimeOwner==owner&&e.Active);
    public bool SetEnabled(string id,bool enabled)
    {
        if(_stopped||!_entries.TryGetValue(id,out var e)||e.Enabled==enabled)return false;
        e.Enabled=enabled;e.Active=enabled&&!_locked();e.Module.SetActivity(e.Active,enabled?ActivityReason.Enabled:ActivityReason.Disabled);Changed?.Invoke(id);return true;
    }
    public void RefreshLockdown()
    {
        if(_stopped)return;
        foreach(var e in _entries.Values){bool active=e.Enabled&&!_locked();if(e.Active==active)continue;e.Active=active;e.Module.SetActivity(active,active?ActivityReason.Resume:ActivityReason.Lockdown);}
        Changed?.Invoke(null);
    }
    public void ApplyReactions(SystemOptions system,double delta){if(_stopped)return;foreach(var e in _entries.Values)if(e.Active)e.Module.ApplyReactions(system,delta);}
    public Entry? EffectSource=>_stopped?null:_entries.Values.FirstOrDefault(e=>e.Active&&e.Module.Stress>=80);
    public IReadOnlyDictionary<string,bool> Settings=>_entries.ToDictionary(p=>p.Key,p=>p.Value.Enabled);
    public void Reset(){if(_stopped)return;foreach(var e in _entries.Values)if(e.Active)e.Module.Reset();}
    public void Shutdown(){if(_stopped)return;_stopped=true;foreach(var e in _entries.Values.Reverse()){e.Active=false;e.Module.Shutdown();}Changed=null;}
}
