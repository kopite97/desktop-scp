using Godot;
using SiteDesktop.Platform.Windows;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Overlay;

// Main owns one coordinator. Controllers publish membership, never native Z-order.
public partial class OverlayStackCoordinator : Node
{
    public readonly record struct Entry(Window Window,int Layer,int Order,bool Dragged=false);
    private readonly Dictionary<string,Entry[]> _groups=new();
    private nint[] _last=System.Array.Empty<nint>();
    private double _age;
    public int AppliedBatches { get; private set; }
    public OverlayStackCoordinator(){ProcessPriority=1000;}
    public void Update(string owner,IEnumerable<Entry> entries)=>_groups[owner]=entries.ToArray();
    public void Remove(string owner)=>_groups.Remove(owner);
    public override void _Process(double delta)
    {
        if(DisplayServer.GetName()=="headless")return;
        _age+=delta;Reconcile(_age>=.05);if(_age>=.05)_age=0;
    }
    public void Reconcile(bool inspect=true)
    {
        if(DisplayServer.GetName()=="headless")return;
        // Query handles only while visible; Hide/Show can destroy/recreate HWNDs.
        var desired=_groups.Values.SelectMany(x=>x)
            .Where(e=>GodotObject.IsInstanceValid(e.Window)&&!e.Window.IsQueuedForDeletion()&&e.Window.Visible)
            .OrderByDescending(e=>e.Dragged).ThenByDescending(e=>e.Layer).ThenByDescending(e=>e.Order)
            .Select(e=>(nint)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,e.Window.GetWindowId()))
            .Where(h=>h!=0).Distinct().ToArray();
        bool changed=!desired.SequenceEqual(_last);
        if(desired.Length>0&&(changed||(inspect&&!WindowsDesktop.StackMatches(desired))))
        {WindowsDesktop.ApplyStack(desired);AppliedBatches++;}
        _last=desired;
    }
    public override void _ExitTree(){_groups.Clear();_last=System.Array.Empty<nint>();}
}
