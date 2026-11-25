using Godot;
using SiteDesktop.Companions;
using SiteDesktop.Desktop;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Overlay;

public partial class Scp999Controller : Node
{
    public bool InitialActive { get; init; }=true;
    public Scp999State State { get; private set; }=null!;
    public StressState Stress { get; init; }=null!;
    public OverlayStackCoordinator Stack { get; init; }=null!;
    public Scp999Window? BodyWindow { get; private set; }
    public IReadOnlyCollection<ScpEffectWindow> EffectWindows => _effects.Values;
    public string InputStatus { get; private set; }="inactive";
    public bool KeyboardMonitoring => _keyboard.Running;
    public bool AutomaticInput { get; set; }=true;
    private readonly KeyboardActivityMonitor _keyboard=new();
    private readonly Dictionary<int,ScpEffectWindow> _effects=new();
    private Texture2D _reactionTexture=null!,_trailTexture=null!;
    public WindowsWindowObserver Observer=>Environment.Observer;
    public void RefreshPlatforms()
    {
        if(_stopped||!State.Visible)return;
        State.ObserveWindows(Environment.Capture(0,true).Windows);Publish();

    }
    public WindowsDesktopEnvironmentSource Environment { get; init; }=null!;
    private long _environmentVersion=-1;
    private ActorWindowLifetime _lifetime=null!;
    private bool _stopped;
    public override void _Ready()
    {
        _lifetime=new(Stack,"999");
        if(Stress==null)throw new InvalidOperationException("The application must supply its common stress state.");
        var body=new SpriteAnimationSet(Scp999Window.AssetPath+"Scp999.contract.json");
        const string vfx="res://Assets/Sprites/vfx/";
        var reactions=new SpriteAnimationSet(vfx+"Scp999Reaction/Scp999Reaction.contract.json");
        var trails=new SpriteAnimationSet(vfx+"Scp999Trail/Scp999Trail.contract.json");
        bool native=OperatingSystem.IsWindows() && DisplayServer.GetName()!="headless";
        var screens=Environment.Current.Screens;
        State=new(screens,body,reactions,trails,Stress);State.SetVisible(InitialActive);
        if(!native)return;
        _reactionTexture=Scp999Window.LoadPng(vfx+"Scp999Reaction/Scp999Reaction.png");
        _trailTexture=Scp999Window.LoadPng(vfx+"Scp999Trail/Scp999Trail.png");
        BodyWindow=new Scp999Window();BodyWindow.BodyPressed+=OnPressed;_lifetime!.Attach(this,BodyWindow);
        if(InitialActive)StartKeyboard();Publish();GD.Print("SCP999_STARTED body=r005 reaction=r001 trail=r001-approved");
    }
    private void StartKeyboard()
    {
        if(!AutomaticInput)return;
        try{_keyboard.Start();InputStatus="activity only";}
        catch(Exception ex){InputStatus="unavailable";GD.PushWarning("Keyboard activity unavailable: "+ex.GetType().Name);}
    }
    private void OnPressed(MouseButton button,Vector2I point)
    {
        if(button==MouseButton.Left)State.BeginPet(point);else if(button==MouseButton.Right)State.BeginDrag(point);
        Publish();
    }
    public void RuntimeTick(double delta,DesktopEnvironmentSnapshot snapshot)
    {
        if(_stopped||!AutomaticInput||BodyWindow==null)return;
        if(!State.Screens.SequenceEqual(snapshot.Screens))State.ReplaceScreens(snapshot.Screens);
        if(!State.Visible)return;
        if(_environmentVersion!=snapshot.Version){State.ObserveWindows(snapshot.Windows);_environmentVersion=snapshot.Version;}
        if(_keyboard.Consume())State.TypingActivity();
        if(snapshot.Pointer is {} pointer)
            State.Tick(delta,pointer,snapshot.Left,snapshot.Right,BodyWindow.ContainsDesktop(pointer));
        else {State.EndGesture();State.Tick(delta,new Vector2(float.NaN,float.NaN));}
        Publish();
    }
    public void Advance(double seconds,Vector2 pointer,bool left=false,bool right=false,bool overBody=false)
    {if(_stopped)return;State.Tick(seconds,pointer,left,right,overBody);Publish();}
    public void Publish()
    {
        if(_stopped||BodyWindow==null)return;
        foreach(int id in _effects.Keys.Where(id=>!State.Effects.Any(e=>e.Id==id)).ToArray())
        {_lifetime!.Release(_effects[id]);_effects.Remove(id);}
        foreach(var e in State.Effects)
        {
            if(!_effects.TryGetValue(e.Id,out var window))
            {
                bool reaction=e.Kind==CompanionEffectKind.Reaction;
                window=new ScpEffectWindow(e,reaction?_reactionTexture:_trailTexture,reaction?State.Reactions:State.Trails);_lifetime!.Attach(this,window);_effects.Add(e.Id,window);
            }
            window.Publish(State);
        }
        BodyWindow.Publish(State);
        bool dragging=State.Gesture==CompanionGesture.Drag;
        var entries=new List<OverlayStackCoordinator.Entry>{new(BodyWindow,10,0,dragging)};
        foreach(var w in _effects.Values)
            entries.Add(new(w,w.Effect.Kind==CompanionEffectKind.Reaction?40:0,w.Effect.Id,dragging&&w.Effect.Kind==CompanionEffectKind.Reaction));
        Stack.Update("999",entries);
    }
    public void SetVisible(bool visible,bool cancelBehavior=false)
    {
        if(_stopped)return;if(visible&&BodyWindow!=null&&!State.Screens.SequenceEqual(Environment.Capture(0,true).Screens))State.ReplaceScreens(Environment.Current.Screens);State.SetVisible(visible,cancelBehavior);
        if(!visible){_keyboard.Dispose();InputStatus="paused";}else if(BodyWindow!=null){RefreshPlatforms();StartKeyboard();}
        Publish();GD.Print(visible?"SCP999_SHOWN":"SCP999_HIDDEN");
    }
    public void ResetPositions(){if(_stopped)return;State.ResetPosition();Publish();}
    public void Shutdown()
    {
        if(_stopped)return;_stopped=true;_lifetime?.Dispose();State?.Shutdown();_keyboard.Dispose();InputStatus="stopped";
        foreach(var w in _effects.Values){_lifetime!.Release(w);}_effects.Clear();
        if(BodyWindow!=null){BodyWindow.BodyPressed-=OnPressed;_lifetime!.Release(BodyWindow);BodyWindow=null;}
        GD.Print("SCP999_STOPPED");
    }
    public override void _ExitTree()=>Shutdown();
}
