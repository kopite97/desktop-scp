using System;
using System.Collections.Generic;

namespace SiteDesktop.Behavior;

public sealed record BehaviorTransition(long Sequence,string? From,string To,string Reason);

// One machine and state instances per actor. Requests made by callbacks are committed
// only at a tick boundary; no recursive Enter/Exit chains.
public sealed class StateMachine
{
    public IBehaviorState? Current { get; private set; }
    public double Age { get; private set; }
    public long Generation { get; private set; }
    public IReadOnlyCollection<BehaviorTransition> Transitions=>_trace;
    private readonly Queue<BehaviorTransition> _trace=new();
    private (IBehaviorState State,string Reason)? _pending;
    private bool _ticking;
    public void Request(IBehaviorState next,string reason)
    {
        if(Current==next&&_pending==null)return;
        _pending=(next,reason);
    }
    public void Tick(double delta)
    {
        if(!double.IsFinite(delta)||delta<0)throw new ArgumentOutOfRangeException(nameof(delta));
        if(_ticking)throw new InvalidOperationException("Reentrant behavior tick");
        _ticking=true;
        try{Commit();Age+=delta;Current?.Tick(delta);Commit();}
        finally{_ticking=false;}
    }
    private void Commit()
    {
        if(_pending is not {} pending)return;
        _pending=null;if(Current==pending.State)return;
        string? previous=Current?.Name;Current?.Exit();Current=pending.State;Age=0;Generation++;
        _trace.Enqueue(new(Generation,previous,Current.Name,pending.Reason));if(_trace.Count>64)_trace.Dequeue();Current.Enter();
    }
    public void Cancel()
    {
        // Invalidate callbacks carrying the previous generation, even when already idle.
        Generation++;_pending=null;Current?.Exit();Current=null;Age=0;_pending=null;
    }
}

public sealed class ActionState(string name,Action<double> tick,Action? enter=null,Action? exit=null) : IBehaviorState
{
    public string Name=>name;
    public void Enter()=>enter?.Invoke();
    public void Tick(double delta)=>tick(delta);
    public void Exit()=>exit?.Invoke();
}
