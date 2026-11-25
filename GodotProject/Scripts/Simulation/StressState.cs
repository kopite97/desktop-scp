using Godot;
using System;
using System.Collections.Generic;

namespace SiteDesktop.Simulation;

public sealed class StressTarget
{
    public string Id { get; }
    public Vector2 Position { get; set; }
    public float Value { get; private set; }
    public bool Active { get; set; }=true;
    public StressTarget(string id, Vector2 position, float value) { Id=id; Position=position; Set(value); }
    public void Set(float value)
    {
        if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
        Value=Math.Clamp(value,0,100);
    }
}

// Main owns stress targets shared by SCP-999 relief and active SCP companions.
public sealed class StressState
{
    private readonly Dictionary<string,StressTarget> _targets=new();
    public IReadOnlyDictionary<string,StressTarget> Targets => _targets;
    public void Add(StressTarget target) => _targets.Add(target.Id,target);
    public bool Remove(string id) => _targets.Remove(id);
    public int Relieve(Vector2 origin,float radius,float amount)
    {
        if (!float.IsFinite(radius)||radius<0||!float.IsFinite(amount)||amount<0) throw new ArgumentOutOfRangeException();
        int changed=0;
        foreach(var target in _targets.Values)
            if(target.Active && target.Position.DistanceSquaredTo(origin)<=radius*radius && target.Value>0)
            { target.Set(target.Value-amount);changed++; }
        return changed;
    }
}
