using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Overlay;
using System;
using System.Collections.Generic;

namespace SiteDesktop.App;
// Owns scheduling only. Modules register their existing product update functions.
public partial class ScpRuntime : Node
{
    public IDesktopEnvironmentSource Environment { get; init; }=null!;
    private readonly List<Action<double,DesktopEnvironmentSnapshot>> _ticks=new();
    private bool _stopped;
    public long Updates { get; private set; }
    public void Register(Action<double,DesktopEnvironmentSnapshot> tick)=>_ticks.Add(tick);
    public override void _Process(double delta)
    {
        if(DisplayServer.GetName()=="headless")return;
        Advance(delta);
    }
    public void Advance(double delta)
    {
        if(_stopped)return;
        var snapshot=Environment.Capture(delta);Updates++;
        foreach(var tick in _ticks)tick(delta,snapshot);
    }
    public void Shutdown(){_stopped=true;SetProcess(false);_ticks.Clear();}
}
