using Godot;
using SiteDesktop.Companions;
using SiteDesktop.Platform.Windows;

namespace SiteDesktop.Overlay;

public partial class ScpEffectWindow : Window
{
    private Sprite2D _sprite=null!;
    public CompanionEffect Effect { get; }
    private readonly Texture2D _texture;
    private readonly SpriteAnimationSet _animation;
    public int DisplayedFrame { get; private set; }
    public ScpEffectWindow(CompanionEffect effect,Texture2D texture,SpriteAnimationSet animation)
    {
        Effect=effect;_texture=texture;_animation=animation;
        Title=effect.Kind==CompanionEffectKind.Trail?"SCP residue":"SCP reaction";
        Visible=false;ForceNative=true;Transient=false;Borderless=true;Unresizable=true;AlwaysOnTop=true;Unfocusable=true;
        Transparent=true;TransparentBg=true;MousePassthrough=true;InitialPosition=WindowInitialPosition.Absolute;
        ContentScaleMode=ContentScaleModeEnum.Disabled;Size=animation.Cell*2;
    }
    public override void _Ready()
    {
        _sprite=new Sprite2D {Texture=_texture,Hframes=_animation.Columns,Vframes=_animation.Rows,Position=_animation.Cell,Scale=new(2,2),TextureFilter=CanvasItem.TextureFilterEnum.Nearest,
            Material=new CanvasItemMaterial {LightMode=CanvasItemMaterial.LightModeEnum.Unshaded,BlendMode=CanvasItemMaterial.BlendModeEnum.Mix}};
        AddChild(_sprite);
    }
    public void Publish(Scp999State state)
    {
        DisplayedFrame=state.EffectFrame(Effect);_sprite.Frame=DisplayedFrame;
        _sprite.FlipH=Effect.Kind==CompanionEffectKind.Trail&&Effect.Flipped;
        Position=WindowsDesktop.ToGodotPosition((Vector2I)(Effect.Position-(Vector2)_animation.Cell).Round());
        if(!Visible){Show();WindowsDesktop.MakeDecorativeClickThrough(this);}
    }
}
