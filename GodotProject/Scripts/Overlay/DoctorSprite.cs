using Godot;
using SiteDesktop.Simulation;
using System;

namespace SiteDesktop.Overlay;

// Exact alpha masks include disconnected tools; a single outer polygon would fill transparent gaps.
public partial class DoctorSprite : Sprite2D
{
    private ImageTexture? _texture;
    private readonly byte[][] _alpha=new byte[42][];
    public int DisplayedFrame=>Frame;
    public override void _Ready()
    {
        using var sheet=GD.Load<Texture2D>(DoctorAnimation.AssetPath+"Scp049.png").GetImage();
        if(sheet.GetSize()!=new Vector2I(448,384))throw new InvalidOperationException("Scp049 r001 sheet dimensions differ");
        sheet.Convert(Image.Format.Rgba8);byte[] rgba=sheet.GetData();
        for(int f=0;f<42;f++)
        {
            _alpha[f]=new byte[64*64];int sx=f%7*64,sy=f/7*64;
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)_alpha[f][y*64+x]=rgba[((sy+y)*448+sx+x)*4+3];
        }
        _texture=ImageTexture.CreateFromImage(sheet);Texture=_texture;Hframes=7;Vframes=6;
        Position=new(64,64);Scale=new(2,2);TextureFilter=TextureFilterEnum.Nearest;
        Material=new CanvasItemMaterial{LightMode=CanvasItemMaterial.LightModeEnum.Unshaded,BlendMode=CanvasItemMaterial.BlendModeEnum.Mix};
        Modulate=SelfModulate=Colors.White;
    }
    public void Publish(DoctorAnimation state){Frame=state.Frame;FlipH=state.Flipped;}
    public bool Hit(Vector2 local)
    {
        if(local.X<0||local.Y<0||local.X>=128||local.Y>=128)return false;
        int x=(int)(local.X/2),y=(int)(local.Y/2);if(FlipH)x=63-x;
        return _alpha[DisplayedFrame][y*64+x]>=16;
    }
    public override void _ExitTree(){Texture=null;_texture?.Dispose();_texture=null;}
}
