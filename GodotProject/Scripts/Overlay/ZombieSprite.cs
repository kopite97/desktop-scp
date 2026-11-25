using Godot;
using SiteDesktop.Simulation;
using System;

namespace SiteDesktop.Overlay;

// One resource owner per controller; masks/textures are never rebuilt during Publish.
public sealed class PlagueArt : IDisposable
{
    public ImageTexture Left { get; }
    public ImageTexture Right { get; }
    public ImageTexture Aura { get; }
    private readonly byte[][][] _alpha={new byte[30][],new byte[30][]};
    public PlagueArt()
    {
        ImageTexture LoadFoot(string file,int slot)
        {
            using var image=GD.Load<Texture2D>(ZombieAnimation.AssetPath+file).GetImage();
            if(image.GetSize()!=new Vector2I(192,160))throw new InvalidOperationException("ART-006 dimensions differ");
            image.Convert(Image.Format.Rgba8);var data=image.GetData();
            for(int f=0;f<30;f++){_alpha[slot][f]=new byte[1024];for(int y=0;y<32;y++)for(int x=0;x<32;x++)_alpha[slot][f][y*32+x]=data[(((f/6*32+y)*192)+f%6*32+x)*4+3];}
            return ImageTexture.CreateFromImage(image);
        }
        Left=LoadFoot("Scp0492_left-foot.png",0);Right=LoadFoot("Scp0492_right-foot.png",1);
        using var aura=GD.Load<Texture2D>(PlaguePresentation.EffectPath+"Scp049Mutation.png").GetImage();
        if(aura.GetSize()!=new Vector2I(192,192))throw new InvalidOperationException("ART-007 dimensions differ");
        Aura=ImageTexture.CreateFromImage(aura);
    }
    public bool FootHit(Vector2 point,int root,int slot,int frame,bool flip)
    {
        int x=(int)Math.Floor((point.X-root)/2),y=(int)Math.Floor((point.Y-56)/2)+28;
        x=flip?16-x:16+x;
        return x>=0&&x<32&&y>=0&&y<32&&_alpha[slot][frame][y*32+x]>=16;
    }
    public static CanvasItemMaterial Material()=>new(){LightMode=CanvasItemMaterial.LightModeEnum.Unshaded,BlendMode=CanvasItemMaterial.BlendModeEnum.Mix};
    public void Dispose(){Left.Dispose();Right.Dispose();Aura.Dispose();}
}

public partial class ZombieSprite : Node2D
{
    private readonly PlagueArt _art;
    private Sprite2D _left=null!,_right=null!;
    public int Frame { get; private set; }
    public bool Flipped { get; private set; }
    public int LeftRoot { get; private set; }=23;
    public int RightRoot { get; private set; }=41;
    public ZombieSprite(PlagueArt art){_art=art;}
    public override void _Ready()
    {
        Sprite2D Foot(Texture2D texture)=>new(){Texture=texture,Hframes=6,Vframes=5,Scale=new(2,2),TextureFilter=TextureFilterEnum.Nearest,Material=PlagueArt.Material()};
        _left=Foot(_art.Left);_right=Foot(_art.Right);AddChild(_left);AddChild(_right);
    }
    public void Publish(DesktopIconAppearance.Appearance? icon,int frame,bool flip)
    {
        Frame=frame;Flipped=flip;LeftRoot=icon?.LeftRoot??23;RightRoot=icon?.RightRoot??41;
        Visible=icon==null||icon.HasPixels;
        _left.Frame=_right.Frame=frame;_left.FlipH=_right.FlipH=flip;
        // Sprite2D mirrors around15.5. Add one source pixel to preserve the authored root x16.
        _left.Position=new(LeftRoot+(flip?2:0),32);_right.Position=new(RightRoot+(flip?2:0),32);
    }
    public bool Hit(Vector2 point)=>Visible&&(_art.FootHit(point,LeftRoot,0,Frame,Flipped)||_art.FootHit(point,RightRoot,1,Frame,Flipped));
}
