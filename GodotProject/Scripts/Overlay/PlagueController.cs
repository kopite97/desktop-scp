using Godot;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System.Collections.Generic;

namespace SiteDesktop.Overlay;

// Owns only SCP-049. The compatibility accessors forward to the shared world's owner.
public partial class PlagueController : Node
{
    public StressState Stress { get; init; }=null!;
    public OverlayStackCoordinator Stack { get; init; }=null!;
    public DesktopObjectController Objects { get; init; }=null!;
    public bool InitialActive { get; init; }=true;
    public PlagueState State { get; private set; }=null!;
    public PlaguePresentation Presentation=>Objects.Presentation;
    public PlagueActorWindow? DoctorWindow { get; private set; }
    public IReadOnlyDictionary<int,PlagueActorWindow> ObjectWindows=>Objects.ObjectWindows;
    public IReadOnlyDictionary<int,PlagueEffectWindow> AuraWindows=>Objects.AuraWindows;
    public IReadOnlyDictionary<int,PlagueEffectWindow> FadingWindows=>Objects.FadingWindows;
    public WindowsWindowObserver Observer=>Objects.Observer;
    public bool Automatic { get=>Objects.Automatic; set=>Objects.Automatic=value; }
    private bool _stopped;
    private double _observeAge;
    public override void _Ready()
    {
        State=new(Objects.State.Screens,Stress,world:Objects.State);State.SetDoctorActive(InitialActive,!InitialActive);Objects.AttachDoctor(State);
        if(DisplayServer.GetName()=="headless")return;
        DoctorWindow=new(0);DoctorWindow.Pressed+=Pressed;AddChild(DoctorWindow);Refresh();PublishDoctor();
        GD.Print("SCP049_STARTED body=ART-004-r001");
    }
    private void Pressed(int id,MouseButton button,Vector2 pointer){if(button==MouseButton.Right)State.BeginDoctorDrag(pointer);Publish();}
    public void Refresh(){if(_stopped)return;Objects.Refresh();if(State.Visible)State.ObserveDoctor(Objects.State.Observations);PublishDoctor();}
    public override void _Process(double delta)
    {
        if(_stopped||!Automatic||DoctorWindow==null||!State.Visible)return;
        if((_observeAge+=delta)>=.05){_observeAge=0;State.ObserveDoctor(Objects.State.Observations);}
        if(State.Doctor.Screens!=Objects.State.Screens)State.ReplaceScreens(Objects.State.Screens);
        if(State.Phase==DoctorPhase.Dragging){if(WindowsDesktop.TryGetPointer(out var p,out _))State.Drag(p);if(!WindowsDesktop.RightPressed)State.EndDrag();}
        State.TickDoctor(delta);PublishDoctor();
    }
    public void Advance(double delta){if(_stopped)return;State.Tick(delta);Publish();}
    private void PublishDoctor(){if(_stopped||DoctorWindow==null)return;DoctorWindow.Publish(State);Stack.Update("049",new[]{new OverlayStackCoordinator.Entry(DoctorWindow,30,0,State.Phase==DoctorPhase.Dragging)});}
    public void Publish(){PublishDoctor();Objects.Publish();}
    public void SpawnData()=>Objects.SpawnData();
    public void SetActive(bool active,bool disabled){if(_stopped)return;State.SetDoctorActive(active,disabled);if(active)Refresh();else PublishDoctor();}
    public void SetVisible(bool visible){SetActive(visible,false);Objects.SetActivity(visible,visible);}
    public void ResetDoctor(){if(_stopped)return;State.ResetDoctor();Publish();}
    public void Shutdown(){if(_stopped)return;_stopped=true;Stack.Remove("049");State?.Shutdown();if(DoctorWindow!=null){DoctorWindow.Pressed-=Pressed;DoctorWindow.Hide();DoctorWindow.QueueFree();DoctorWindow=null;}GD.Print("SCP049_STOPPED");}
    public override void _ExitTree()=>Shutdown();
}
