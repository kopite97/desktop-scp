using Godot;
using System;
using System.Collections.Generic;

namespace SiteDesktop.Overlay;
// Native windows and their stack registration share one shutdown owner.
public sealed class ActorWindowLifetime(OverlayStackCoordinator stack,string owner) : IDisposable
{
    private readonly HashSet<Window> _windows=new();
    public void Attach(Node parent,Window window){_windows.Add(window);parent.AddChild(window);}
    public void Release(Window window)
    {
        if(!_windows.Remove(window)||!GodotObject.IsInstanceValid(window))return;
        window.Hide();window.QueueFree();
    }
    public void Dispose(){stack.Remove(owner);foreach(var w in new List<Window>(_windows))Release(w);}
}
