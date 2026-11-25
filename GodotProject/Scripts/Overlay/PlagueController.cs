using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System.Collections.Generic;

namespace SiteDesktop.Overlay;

// Owns only SCP-049. Diagnostic orchestration lives outside product actors.
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
    public WindowsDesktopEnvironmentSource Environment { get; init; }=null!;
    private long _environmentVersion=-1;
    private ActorWindowLifetime _lifetime=null!;
    private bool _stopped;
    public PortalPresentation? Portals { get; private set; }

    public override void _Ready()
    {
        _lifetime=new(Stack,"049");
        State=new(Objects.State.Screens,Stress,world:Objects.State);State.SetDoctorActive(InitialActive,!InitialActive);Objects.Presentation.Treatment=()=>State.TreatmentView;
        if(DisplayServer.GetName()=="headless")return;
        Portals=new(this,Stack,"049-portals");DoctorWindow=new(0);DoctorWindow.Pressed+=Pressed;_lifetime!.Attach(this,DoctorWindow);RefreshBody();PublishBody();
        GD.Print("SCP049_STARTED body=ART-004-r001");
    }
    private void Pressed(int id,MouseButton button,Vector2 pointer){if(button==MouseButton.Right)State.BeginDoctorDrag(pointer);PublishBody();}
    public void RefreshBody(){if(_stopped)return;if(State.Visible)State.ObserveDoctor(Environment.Capture(0,true).Windows);PublishBody();}
    public void RuntimeTick(double delta,DesktopEnvironmentSnapshot snapshot)
    {
        if(_stopped||!Automatic||DoctorWindow==null||!State.Visible)return;
        if(State.Doctor.Screens!=snapshot.Screens)State.ReplaceDoctorScreens(snapshot.Screens);
        if(_environmentVersion!=snapshot.Version){State.ObserveDoctor(snapshot.Windows);_environmentVersion=snapshot.Version;}
        if(State.Phase==DoctorPhase.Dragging){if(snapshot.Pointer is {} p)State.Drag(p);if(!snapshot.Right)State.EndDrag();}
        State.TickDoctor(delta);PublishBody();
    }
    public void PublishBody(){if(_stopped||DoctorWindow==null)return;Portals?.Publish(State.Behavior.Portal.Frame,State.Visible&&!State.Stopped);DoctorWindow.Publish(State);Stack.Update("049",new[]{new OverlayStackCoordinator.Entry(DoctorWindow,30,0,State.Phase==DoctorPhase.Dragging)});}
    public void SpawnData()=>Objects.SpawnData();
    public void SetActive(bool active,bool disabled){if(_stopped)return;State.SetDoctorActive(active,disabled);if(active)RefreshBody();else PublishBody();}
    public void ResetDoctor(){if(_stopped)return;State.ResetDoctor();PublishBody();}
    public void Shutdown(){if(_stopped)return;_stopped=true;Portals?.Dispose();_lifetime?.Dispose();State?.Shutdown();if(DoctorWindow!=null){DoctorWindow.Pressed-=Pressed;_lifetime!.Release(DoctorWindow);DoctorWindow=null;}GD.Print("SCP049_STOPPED");}
    public override void _ExitTree()=>Shutdown();
}
