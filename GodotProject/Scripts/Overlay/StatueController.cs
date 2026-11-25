using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System.Linq;

namespace SiteDesktop.Overlay;
public partial class StatueController : Node
{
    public StressState SharedStress { get; init; }=null!;
    public OverlayStackCoordinator Stack { get; init; }=null!;
    public DesktopObjectController Objects { get; init; }=null!;
    public bool InitialActive { get; init; }=true;
    public StatueState State { get; private set; }=null!;
    public Window? Actor { get; private set; }
    public bool Automatic { get; set; }=true;
    public bool Noisy { get; set; }
    private StatueGlyph? _glyph;

    public WindowsDesktopEnvironmentSource Environment { get; init; }=null!;
    private long _environmentVersion=-1;
    private ActorWindowLifetime _lifetime=null!;
    private bool _stopped;
    public override void _Ready()
    {
        _lifetime=new(Stack,"173");
        State=new(Objects.State.Screens,SharedStress);State.SetVisible(InitialActive);
        if(DisplayServer.GetName()=="headless")return;
        Actor=new Window{Title="SCP-173",Visible=false,ForceNative=true,Transient=false,Borderless=true,Unresizable=true,AlwaysOnTop=true,Unfocusable=true,Transparent=true,TransparentBg=true,Size=new(64,96),InitialPosition=Window.WindowInitialPosition.Absolute};
        Actor.MousePassthroughPolygon=new Vector2[]{new(16,4),new(48,4),new(54,34),new(58,66),new(48,90),new(14,90),new(6,66),new(10,34)};
        _glyph=new StatueGlyph{Material=new CanvasItemMaterial{LightMode=CanvasItemMaterial.LightModeEnum.Unshaded}};Actor.AddChild(_glyph);Actor.WindowInput+=Input;_lifetime!.Attach(this,Actor);Publish();
        GD.Print("SCP173_STARTED art=temporary-code");
    }
    private void Input(InputEvent e)
    {if(e is InputEventMouseButton{Pressed:true,ButtonIndex:MouseButton.Right} b)State.BeginDrag(WindowsDesktop.GetBounds(Actor!).Position+b.Position);}
    public void RuntimeTick(double delta,DesktopEnvironmentSnapshot snapshot)
    {
        if(_stopped||!Automatic||Actor==null||!State.Visible)return;
        if(!State.Physics.Screens.SequenceEqual(snapshot.Screens)){State.EndDrag();State.Physics.ReplaceScreens(snapshot.Screens);}
        if(_environmentVersion!=snapshot.Version){State.Physics.Observe(snapshot.Windows);_environmentVersion=snapshot.Version;}
        if(State.Dragging){if(snapshot.Pointer is {} p)State.Drag(p);if(!snapshot.Right)State.EndDrag();}
        State.Tick(delta,snapshot.Pointer,snapshot.AllScreens,Objects.State);Publish();
    }
    public void Publish()
    {
        if(Actor==null)return;
        if(!State.Visible||State.Stopped){Actor.Hide();return;}
        Actor.Position=WindowsDesktop.ToGodotPosition((Vector2I)State.Position.Round());_glyph!.Observed=State.Observed;_glyph.Noisy=Noisy;_glyph.QueueRedraw();Actor.Show();
        Stack.Update("173",new[]{new OverlayStackCoordinator.Entry(Actor,30,173,State.Dragging)});
    }
    public void SetVisible(bool visible){if(_stopped)return;if(visible){State.Physics.ReplaceScreens(Objects.State.Screens);State.Physics.Observe(Environment.Capture(0,true).Windows);}State.SetVisible(visible);Publish();}
    public void Shutdown(){if(_stopped)return;_stopped=true;State?.Shutdown();_lifetime?.Dispose();if(Actor!=null){Actor.WindowInput-=Input;_lifetime!.Release(Actor);Actor=null;}GD.Print("SCP173_STOPPED");}
    public override void _ExitTree()=>Shutdown();
}
public partial class StatueGlyph : Node2D
{
    public bool Observed;
    public bool Noisy;
    private void Box(float x,float y,float w,float h,string hex)=>DrawRect(new(x,y,w,h),new Color(hex));
    public override void _Draw()
    {
        Box(16,4,32,34,"252525");Box(20,8,24,26,"ccc7ae");Box(22,12,6,12,"913b36");Box(36,12,5,10,"913b36");Box(27,22,10,7,"556348");
        Box(12,36,40,40,"252525");Box(16,38,32,34,"b1aa91");Box(6,42,10,24,"aaa38a");Box(48,42,10,24,"aaa38a");
        Box(14,72,14,18,"252525");Box(36,72,12,18,"252525");Box(18,74,7,12,"aaa38a");Box(39,74,6,12,"aaa38a");
        if(Observed){Box(25,16,2,2,"fff0d0");Box(37,16,2,2,"fff0d0");}
        if(Noisy){float y=(Time.GetTicksMsec()/120%2)==0?30:32;Box(16,y,8,4,"913b36");Box(40,y,8,4,"913b36");}
    }
}
