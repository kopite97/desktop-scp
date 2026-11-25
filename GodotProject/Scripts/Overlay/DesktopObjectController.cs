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
    public PlagueState? Doctor { get; private set; }
    public OverlayStackCoordinator Stack { get; init; }=null!;
    public DesktopIconAppearance? Icons { get; init; }
    public DesktopObjectState State { get; private set; }=null!;
    public PlaguePresentation Presentation { get; private set; }=null!;
    public IReadOnlyDictionary<int,PlagueActorWindow> ObjectWindows=>_windows;
    public WindowsWindowObserver Observer { get; }=new();
    public bool Automatic { get; set; }=true;
    private readonly Dictionary<int,PlagueActorWindow> _windows=new();
    private readonly Dictionary<int,PlagueEffectWindow> _auras=new(),_fading=new();
    public IReadOnlyDictionary<int,PlagueEffectWindow> AuraWindows=>_auras;
    public IReadOnlyDictionary<int,PlagueEffectWindow> FadingWindows=>_fading;
    private PlagueArt? _art;
    private bool _stopped;
    private double _observeAge,_screenAge;
    private static DesktopScreen[] Screens(bool fallback=false)
    {
        if(DisplayServer.GetName()=="headless")return new[]{DesktopPlacement.Select()};
        if(DesktopPlacement.RequestedMonitor=="")return OverlayController.ReadScreens();
        try{return new[]{DesktopPlacement.Select()};}
        catch(InvalidOperationException)when(fallback){return OverlayController.ReadScreens().Take(1).ToArray();}
    }
    public override void _Ready()
    {
        State=new(Screens());State.SetActivity(InitialActive,InitialZombiesActive);Presentation=new(State);if(Icons!=null)State.ChooseAppearance=Icons.Choose;State.Spawn();
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
        if(_stopped||!State.Visible)return;State.Observe(Observer.Read());Publish();
    }
    public override void _Process(double delta)
    {
        if(_stopped||!Automatic||_art==null)return;
        if((_screenAge+=delta)>=.5){_screenAge=0;var screens=Screens(true);if(!State.Screens.SequenceEqual(screens)){State.ReplaceScreens(screens);Presentation.ReplaceScreens();}}
        if(!State.Visible)return;
        if((_observeAge+=delta)>=.05){_observeAge=0;Refresh();}
        if(State.DraggedObject!=null)
        {
            if(WindowsDesktop.TryGetPointer(out var pointer,out _))State.Drag(pointer);
            if(!WindowsDesktop.RightPressed)State.EndDrag();
        }
        Advance(delta);
    }
    public void Advance(double delta){if(_stopped)return;State.Tick(delta);Publish();}
    public void Publish()
    {
        if(_stopped)return;Presentation.Sync();
        if(_art==null){Icons?.Retain(Presentation.RetainedAppearances);return;}
        foreach(int id in _windows.Keys.Where(id=>!State.Objects.ContainsKey(id)).ToArray())
        {var old=_windows[id];old.Pressed-=Pressed;old.Hide();old.QueueFree();_windows.Remove(id);}
        foreach(var item in State.Objects.Values)
        {
            if(!_windows.TryGetValue(item.Id,out var window)){window=new(item.Id,_art);window.Pressed+=Pressed;AddChild(window);_windows.Add(item.Id,window);}
            window.Icon=Icons?.Find(item.AppearanceId);window.Publish(State,Doctor);
        }
        var entries=new List<OverlayStackCoordinator.Entry>();
        entries.AddRange(_windows.Select(pair=>new OverlayStackCoordinator.Entry(pair.Value,20,pair.Key,State.DraggedObject==pair.Key)));
        var auraStates=Presentation.Auras().ToDictionary(a=>a.Id);
        Rect2I Area(Vector2 foot)=>State.Screens.OrderBy(s=>((Vector2)s.Usable.GetCenter()).DistanceSquaredTo(foot)).First().Usable;
        foreach(int id in _auras.Keys.Where(id=>!auraStates.ContainsKey(id)).ToArray()){_auras[id].Hide();_auras[id].QueueFree();_auras.Remove(id);}
        foreach(var aura in auraStates.Values)
        {
            if(!_auras.TryGetValue(aura.Id,out var w)){w=new(_art!);AddChild(w);_auras.Add(aura.Id,w);}
            w.Publish(aura,Area(aura.Foot),State.Visible&&State.ZombiesActive);entries.Add(new(w,21,aura.Id,aura.Dragged));
        }
        foreach(int id in _fading.Keys.Where(id=>!Presentation.Disappearing.ContainsKey(id)).ToArray()){_fading[id].Hide();_fading[id].QueueFree();_fading.Remove(id);}
        foreach(var pair in Presentation.Disappearing)
        {
            if(!_fading.TryGetValue(pair.Key,out var w)){w=new(_art!,true);AddChild(w);_fading.Add(pair.Key,w);}
            w.Publish(pair.Value,State.ZombieClock,Icons?.Find(pair.Value.Snapshot.AppearanceId),Area(pair.Value.Snapshot.Foot),State.Visible&&State.ZombiesActive);entries.Add(new(w,19,pair.Key));
        }
        Stack.Update("desktop-objects",entries);
        Icons?.Retain(Presentation.RetainedAppearances);
    }
    public void SpawnData(){if(_stopped)return;State.Spawn();Publish();}
    public void SetActivity(bool active,bool zombiesActive,bool clearEffects=false){if(_stopped)return;State.SetActivity(active,zombiesActive,clearEffects);if(clearEffects)Presentation.ReplaceScreens();Icons?.SetPaused(!active);if(active)Refresh();else Publish();}
    public void AttachDoctor(PlagueState doctor){Doctor=doctor;Presentation.Doctor=doctor;}
    public void Shutdown()
    {
        if(_stopped)return;_stopped=true;Stack.Remove("desktop-objects");State?.Shutdown();
        Presentation?.Dispose();
        foreach(var w in _auras.Values.Concat(_fading.Values)){w.Hide();w.QueueFree();}_auras.Clear();_fading.Clear();
        foreach(var window in _windows.Values){window.Pressed-=Pressed;window.Hide();window.QueueFree();}_windows.Clear();
        _art?.Dispose();_art=null;
        GD.Print("DESKTOP_OBJECTS_STOPPED");
    }
    public override void _ExitTree()=>Shutdown();
}
