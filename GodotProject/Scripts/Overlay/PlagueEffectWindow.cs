using Godot;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System;

namespace SiteDesktop.Overlay;

// Entire native window is decorative; clip visuals without changing target/input coordinates.
public partial class PlagueEffectWindow : Window
{
    private readonly PlagueArt _art;
    public bool Disappearance { get; }
    private Control _clip=null!;
    private Node2D _content=null!;
    private Sprite2D? _aura;
    private ZombieSprite? _feet;
    private PlagueGlyph? _body;
    public int DisplayedFrame { get; private set; }
    public Rect2I LocalClip { get; private set; }
    public PlagueEffectWindow(PlagueArt art,bool disappearance=false)
    {
        _art=art;Disappearance=disappearance;Title=disappearance?"SCP fading icon":"SCP treatment aura";
        Visible=false;ForceNative=true;Transient=false;Borderless=true;Unresizable=true;AlwaysOnTop=true;Unfocusable=true;
        Transparent=true;TransparentBg=true;MousePassthrough=true;InitialPosition=WindowInitialPosition.Absolute;
        ContentScaleMode=ContentScaleModeEnum.Disabled;Size=disappearance?new(64,64):new(96,96);
    }
    public override void _Ready()
    {
        _clip=new(){ClipContents=true,MouseFilter=Control.MouseFilterEnum.Ignore};AddChild(_clip);_content=new();_clip.AddChild(_content);
        if(Disappearance){_feet=new(_art);_content.AddChild(_feet);_body=new(){Kind=DummyKind.Zombie,Material=PlagueArt.Material(),TextureFilter=CanvasItem.TextureFilterEnum.Nearest};_content.AddChild(_body);}
        else{_aura=new(){Texture=_art.Aura,Hframes=4,Vframes=4,Position=new(48,48),Scale=new(2,2),TextureFilter=CanvasItem.TextureFilterEnum.Nearest,Material=PlagueArt.Material()};_content.AddChild(_aura);}
    }
    public void Publish(PlagueAura aura,Rect2I workArea,bool visible)
    {
        DisplayedFrame=PlaguePresentation.EffectSet.FrameAt(aura.Tag,aura.Age);_aura!.Frame=DisplayedFrame;
        Place(aura.Foot-new Vector2(48,80),workArea,visible);
    }
    public void Publish(PlagueDisappearance fading,double clock,DesktopIconAppearance.Appearance? icon,Rect2I workArea,bool visible)
    {
        DisplayedFrame=fading.Frame(clock);_feet!.Publish(icon,DisplayedFrame,fading.Snapshot.Flipped);
        _body!.Icon=icon;_body.SelfModulate=new(1,1,1,ZombieAnimation.Set.Frames[DisplayedFrame].BodyAlpha);_body.QueueRedraw();
        Place(fading.Snapshot.Foot-new Vector2(32,64),workArea,visible);
    }
    private void Place(Vector2 top,Rect2I workArea,bool visible)
    {
        var physical=(Vector2I)top.Round();var whole=new Rect2I(physical,Size);var clipped=whole.Intersection(workArea);
        LocalClip=new(clipped.Position-physical,clipped.Size);_clip.Position=LocalClip.Position;_clip.Size=LocalClip.Size;_content.Position=-_clip.Position;
        var position=WindowsDesktop.ToGodotPosition(physical);if(Position!=position)Position=position;
        if(!visible||clipped.Size.X<=0||clipped.Size.Y<=0){Hide();return;}
        if(!Visible){Show();WindowsDesktop.MakeDecorativeClickThrough(this);}
    }
}
