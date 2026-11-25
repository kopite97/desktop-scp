using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Overlay;
public partial class DesktopObjectController : Node
{
    public bool InitialActive { get; init; }=true;
    public bool InitialZombiesActive { get; init; }=true;
    public OverlayStackCoordinator Stack { get; init; }=null!;
    public DesktopIconAppearance? Icons { get; init; }
    public DesktopObjectState State { get; private set; }=null!;
    public PlaguePresentation Presentation { get; private set; }=null!;
    public IReadOnlyDictionary<int,PlagueActorWindow> ObjectWindows=>_windows;
    public WindowsWindowObserver Observer=>Environment.Observer;
    public bool Automatic { get; set; }=true;
    private readonly Dictionary<int,PlagueActorWindow> _windows=new();
    private readonly Dictionary<int,PlagueEffectWindow> _auras=new(),_fading=new();
    public IReadOnlyDictionary<int,PlagueEffectWindow> AuraWindows=>_auras;
    public IReadOnlyDictionary<int,PlagueEffectWindow> FadingWindows=>_fading;
    private PlagueArt? _art;
    public WindowsDesktopEnvironmentSource Environment { get; init; }=null!;
    private long _environmentVersion=-1;
    private ActorWindowLifetime _lifetime=null!;
    private bool _stopped;
    public override void _Ready()
    {
        _lifetime=new(Stack,"desktop-objects");
        State=new(Environment.Current.Screens);State.SetActivity(InitialActive,InitialZombiesActive);Presentation=new(State);if(Icons!=null)State.ChooseAppearance=Icons.Choose;
        if(DisplayServer.GetName()=="headless")return;
        _art=new();Refresh();
        GD.Print("DESKTOP_OBJECTS_STARTED");
    }
    private void Pressed(int id,MouseButton button,Vector2 pointer)
    {
        if(button==MouseButton.Left&&id!=0)State.Collect(id);
        else if(button==MouseButton.Right)State.BeginObjectDrag(id,pointer);
        Publish();
    }
    public void Refresh()
    {
        if(_stopped||!State.Visible)return;State.Observe(Environment.Capture(0,true).Windows);Publish();
    }
    public void RuntimeTick(double delta,DesktopEnvironmentSnapshot snapshot)
    {
        if(_stopped||!Automatic||_art==null)return;
        if(!State.Screens.SequenceEqual(snapshot.Screens)){State.ReplaceScreens(snapshot.Screens);Presentation.ReplaceScreens();}
        if(!State.Visible)return;
        if(_environmentVersion!=snapshot.Version){State.Observe(snapshot.Windows);_environmentVersion=snapshot.Version;}
        if(State.DraggedObject!=null){if(snapshot.Pointer is {} pointer)State.Drag(pointer);if(!snapshot.Right)State.EndDrag();}
        Advance(delta);
    }
    public void Advance(double delta){if(_stopped)return;State.Tick(delta);Publish();}
    public void Publish()
    {
        if(_stopped)return;Presentation.Sync();
        if(_art==null){Icons?.Retain(Presentation.RetainedAppearances);return;}
        foreach(int id in _windows.Keys.Where(id=>!State.Objects.ContainsKey(id)).ToArray())
        {var old=_windows[id];old.Pressed-=Pressed;_lifetime!.Release(old);_windows.Remove(id);}
        foreach(var item in State.Objects.Values)
        {
            if(!_windows.TryGetValue(item.Id,out var window)){window=new(item.Id,_art);window.Pressed+=Pressed;_lifetime!.Attach(this,window);_windows.Add(item.Id,window);}
            window.Icon=Icons?.Find(item.AppearanceId);window.Publish(State);
        }
        var entries=new List<OverlayStackCoordinator.Entry>();
        entries.AddRange(_windows.Select(pair=>new OverlayStackCoordinator.Entry(pair.Value,20,pair.Key,State.DraggedObject==pair.Key)));
        var auraStates=Presentation.Auras().ToDictionary(a=>a.Id);
        Rect2I Area(Vector2 foot)=>State.Screens.OrderBy(s=>((Vector2)s.Usable.GetCenter()).DistanceSquaredTo(foot)).First().Usable;
        foreach(int id in _auras.Keys.Where(id=>!auraStates.ContainsKey(id)).ToArray()){_lifetime!.Release(_auras[id]);_auras.Remove(id);}
        foreach(var aura in auraStates.Values)
        {
            if(!_auras.TryGetValue(aura.Id,out var w)){w=new(_art!);_lifetime!.Attach(this,w);_auras.Add(aura.Id,w);}
            w.Publish(aura,Area(aura.Foot),State.Visible&&State.ZombiesActive);entries.Add(new(w,21,aura.Id,aura.Dragged));
        }
        foreach(int id in _fading.Keys.Where(id=>!Presentation.Disappearing.ContainsKey(id)).ToArray()){_lifetime!.Release(_fading[id]);_fading.Remove(id);}
        foreach(var pair in Presentation.Disappearing)
        {
            if(!_fading.TryGetValue(pair.Key,out var w)){w=new(_art!,true);_lifetime!.Attach(this,w);_fading.Add(pair.Key,w);}
            w.Publish(pair.Value,State.ZombieClock,Icons?.Find(pair.Value.Snapshot.AppearanceId),Area(pair.Value.Snapshot.Foot),State.Visible&&State.ZombiesActive);entries.Add(new(w,19,pair.Key));
        }
        Stack.Update("desktop-objects",entries);
        Icons?.Retain(Presentation.RetainedAppearances);
    }
    public void SpawnData(){if(_stopped)return;State.Spawn();Publish();}
    public void SetActivity(bool active,bool zombiesActive,bool clearEffects=false){if(_stopped)return;State.SetActivity(active,zombiesActive,clearEffects);if(clearEffects)Presentation.ReplaceScreens();Icons?.SetPaused(!active);if(active)Refresh();else Publish();}

    public void Shutdown()
    {
        if(_stopped)return;_stopped=true;_lifetime?.Dispose();State?.Shutdown();
        Presentation?.Dispose();
        foreach(var w in _auras.Values.Concat(_fading.Values)){_lifetime!.Release(w);}_auras.Clear();_fading.Clear();
        foreach(var window in _windows.Values){window.Pressed-=Pressed;_lifetime!.Release(window);}_windows.Clear();
        _art?.Dispose();_art=null;
        GD.Print("DESKTOP_OBJECTS_STOPPED");
    }
    public override void _ExitTree()=>Shutdown();
}
