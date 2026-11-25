using Godot;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System;
using System.Collections.Generic;

namespace SiteDesktop.App.Scps;
public sealed class ScpCreationContext(Action<Node> attach,ScpRuntime runtime,WindowsDesktopEnvironmentSource environment,StressState stress,OverlayStackCoordinator stack)
{
    public WindowsDesktopEnvironmentSource Environment=>environment;
    public StressState Stress=>stress;
    public OverlayStackCoordinator Stack=>stack;
    public DesktopObjectController? Objects { get; set; }
    private readonly Dictionary<string,Node> _controllers=new();
    public T? Find<T>(string id) where T:Node=>_controllers.TryGetValue(id,out var controller)?controller as T:null;
    public void Register(string id,Node controller,Action<double,SiteDesktop.Desktop.DesktopEnvironmentSnapshot> tick)
    {
        if(!_controllers.TryAdd(id,controller))throw new InvalidOperationException("Duplicate controller ID");
        attach(controller);runtime.Register(tick);
    }
}
