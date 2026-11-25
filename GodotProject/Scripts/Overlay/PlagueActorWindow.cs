using Godot;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System;

namespace SiteDesktop.Overlay;

// Approved ART-004 body and ART-006 feet; the original runtime icon stays independent.
public partial class PlagueActorWindow : Window
{
    public int ObjectId { get; }
    public DesktopIconAppearance.Appearance? Icon { get; set; }
    public event Action<int,MouseButton,Vector2>? Pressed;
    private PlagueGlyph _glyph=null!;
    public DoctorSprite? DoctorArt { get; private set; }
    public ZombieSprite? ZombieArt { get; private set; }
    private readonly PlagueArt? _art;
    private bool _transferring;
    private static readonly Vector2[] DataShape={new(8,12),new(44,12),new(56,24),new(56,56),new(8,56)};
    public PlagueActorWindow(int id,PlagueArt? art=null)
    {
        _art=art;
        ObjectId=id;Title=id==0?"SCP-049":"SCP game data";Visible=false;ForceNative=true;Transient=false;
        Borderless=true;Unresizable=true;AlwaysOnTop=true;Unfocusable=true;Transparent=true;TransparentBg=true;
        ContentScaleMode=ContentScaleModeEnum.Disabled;InitialPosition=WindowInitialPosition.Absolute;Size=id==0?new(128,128):new(64,64);
    }
    public override void _Ready()
    {
        if(ObjectId!=0&&_art!=null){ZombieArt=new(_art){Visible=false};AddChild(ZombieArt);}
        _glyph=new PlagueGlyph{Material=PlagueArt.Material(),TextureFilter=CanvasItem.TextureFilterEnum.Nearest};AddChild(_glyph);
        if(ObjectId==0){_glyph.Visible=false;DoctorArt=new();AddChild(DoctorArt);MousePassthroughPolygon=Array.Empty<Vector2>();}
        WindowInput+=OnInput;
    }
    private void OnInput(InputEvent input)
    {
        if(!_transferring&&input is InputEventMouseButton {Pressed:true} b)
        {
            if(Hit(b.Position))Pressed?.Invoke(ObjectId,b.ButtonIndex,WindowsDesktop.GetBounds(this).Position+b.Position);
        }
    }
    public bool Hit(Vector2 local)=>DoctorArt!=null?DoctorArt.Hit(local):(Icon==null?Geometry2D.IsPointInPolygon(local,DataShape):Icon.Hit(local,_glyph.Broken,_glyph.Kind==DummyKind.Zombie))||(ZombieArt?.Visible==true&&ZombieArt.Hit(local));
    public bool ContainsDesktop(Vector2 pointer)=>Visible&&Hit(pointer-(Vector2)WindowsDesktop.GetBounds(this).Position);
    public void Publish(PlagueState doctor)=>Publish(doctor.World,doctor);
    public void Publish(DesktopObjectState state,PlagueState? doctor=null)
    {
        if(state.Stopped||(ObjectId!=0&&!state.Visible)){Hide();return;}
        Vector2 foot;
        if(ObjectId==0)
        {
            if(doctor==null||!doctor.Visible||doctor.Stopped){Hide();return;}
            foot=doctor.Doctor.Body.Foot;DoctorArt!.Publish(doctor.Animation);
            var portal=doctor.Behavior.Portal.Frame;_transferring=portal!=null;
            float alpha=portal?.BodyAlpha??1;float scale=.45f+.55f*alpha;
            DoctorArt.SelfModulate=new Color(1,1,1,alpha);DoctorArt.Scale=new(2*scale,2*scale);DoctorArt.Position=new(64,112-48*scale);
            if(alpha<=0){Hide();return;}
        }
        else
        {
            if(!state.Objects.TryGetValue(ObjectId,out var item)||!state.IsActive(item)){Hide();return;}
            foot=item.Foot;_glyph.Kind=item.Kind;_glyph.Claimed=item.ClaimedBy!=null;
            _glyph.Icon=Icon;
            _glyph.Broken=item.BrokenAge!=null;
            // The polygon remains empty for alpha-based input. Reassigning it
            // on every Publish calls SetWindowRgn(..., TRUE), erasing the native
            // background even when neither the icon nor the cursor has moved.
            if(ZombieArt!=null){if(item.ZombieAnimation is { } animation)ZombieArt.Publish(Icon,animation.Frame,animation.Flipped);else ZombieArt.Visible=false;}
        }
        _glyph.QueueRedraw();
        var position=WindowsDesktop.ToGodotPosition((Vector2I)(foot-(ObjectId==0?new Vector2(64,112):new Vector2(32,_glyph.Kind==DummyKind.Zombie?64:56))).Round());
        if(Position!=position)Position=position;
        if(!Visible)Show();
        if(WindowsDesktop.TryGetPointer(out var pointer,out _))
        {
            bool pass=_transferring||!Hit(pointer-WindowsDesktop.GetBounds(this).Position);
            if(MousePassthrough!=pass)MousePassthrough=pass;
            WindowsDesktop.SetIconClickThrough(this,pass);
        }
    }
    public override void _ExitTree(){WindowInput-=OnInput;Pressed=null;}
}
public partial class PlagueGlyph : Node2D
{
    public bool Claimed,Broken;
    public DummyKind Kind;
    public DesktopIconAppearance.Appearance? Icon;
    private void Box(float x,float y,float w,float h,string color)=>DrawRect(new(x,y,w,h),new Color(color));
    public override void _Draw()
    {
        if(Icon!=null)
        {
            if(Broken)
            {
                int half=Icon.Pixels.Height/2;
                DrawTextureRectRegion(Icon.Texture,new(Icon.Offset+new Vector2(-4,-2),new(Icon.Pixels.Width,half)),new(0,0,Icon.Pixels.Width,half));
                DrawTextureRectRegion(Icon.Texture,new(Icon.Offset+new Vector2(4,half+2),new(Icon.Pixels.Width,Icon.Pixels.Height-half)),new(0,half,Icon.Pixels.Width,Icon.Pixels.Height-half));
            }
            else DrawTexture(Icon.Texture,Kind==DummyKind.Zombie?Icon.ZombieOffset:Icon.Offset);
            if(Claimed)DrawRect(new(3,3,58,58),new Color("e7bf74"),false,1);
        }
        else
        {
            Box(8,12,48,44,Claimed?"af8752":"202b38");
            if(Kind!=DummyKind.Trash){Box(12,16,40,36,"d6d3b9");Box(38,16,14,12,"829594");Box(18,34,28,4,"506574");Box(18,42,20,4,"506574");}
            else{Box(14,22,36,30,"718d91");Box(12,18,40,6,"bac8b7");Box(22,14,20,4,"bac8b7");Box(22,30,4,16,"344b55");Box(36,30,4,16,"344b55");}
            if(Claimed){Box(4,24,4,16,"e7bf74");Box(56,24,4,16,"e7bf74");}
        }
    }
}
