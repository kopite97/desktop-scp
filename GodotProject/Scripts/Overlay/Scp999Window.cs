using Godot;
using SiteDesktop.Companions;
using SiteDesktop.Platform.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SiteDesktop.Overlay;

public partial class Scp999Window : Window
{
    public const string AssetPath="res://Assets/Sprites/characters/Scp999/";
    public event Action<MouseButton,Vector2I>? BodyPressed;
    public int DisplayedFrame { get; private set; }=-1;
    private Sprite2D _body=null!,_outline=null!;
    private CanvasModulate _ambient=null!;
    private PointLight2D _key=null!;
    private readonly Dictionary<(int,bool),Vector2[]> _contours=new();
    private bool _flip;
    private bool _hot;
    public Scp999Window()
    {
        Title="SCP-999";Visible=false;ForceNative=true;Transient=false;Borderless=true;Unresizable=true;
        AlwaysOnTop=true;Unfocusable=true;Transparent=true;TransparentBg=true;
        InitialPosition=WindowInitialPosition.Absolute;ContentScaleMode=ContentScaleModeEnum.Disabled;Size=new(128,128);
    }
    public static ImageTexture LoadPng(string file)
    {
        using var image=GD.Load<Texture2D>(file).GetImage();
        if(image==null||image.IsEmpty())throw new InvalidDataException(file);
        return ImageTexture.CreateFromImage(image);
    }
    public override void _Ready()
    {
        var texture=new CanvasTexture {DiffuseTexture=LoadPng(AssetPath+"Scp999_color.png"),NormalTexture=LoadPng(AssetPath+"Scp999_normal.png"),SpecularTexture=LoadPng(AssetPath+"Scp999_specular.png"),SpecularShininess=.45f,SpecularColor=new Color(.35f,.35f,.35f),TextureFilter=CanvasItem.TextureFilterEnum.Nearest,TextureRepeat=CanvasItem.TextureRepeatEnum.Disabled};
        _body=new Sprite2D {Texture=texture,Hframes=50,Position=new(64,64),Scale=new(2,2),TextureFilter=CanvasItem.TextureFilterEnum.Nearest};AddChild(_body);
        _outline=new Sprite2D {Texture=LoadPng(AssetPath+"Scp999_outline.png"),Hframes=50,Position=new(64,64),Scale=new(2,2),TextureFilter=CanvasItem.TextureFilterEnum.Nearest,Material=new CanvasItemMaterial {LightMode=CanvasItemMaterial.LightModeEnum.Unshaded,BlendMode=CanvasItemMaterial.BlendModeEnum.Mix}};AddChild(_outline);
        _ambient=new CanvasModulate();AddChild(_ambient);
        using var white=Image.CreateEmpty(512,512,false,Image.Format.Rgba8);white.Fill(Colors.White);
        _key=new PointLight2D {Texture=ImageTexture.CreateFromImage(white),Position=new(-36,-1),Height=100,ShadowEnabled=false};AddChild(_key);
        SetLighting(.9f,.5f,new Color(1,.98f,.94f));
        using var combined=GD.Load<Texture2D>(AssetPath+"Scp999_combined.png").GetImage();
        for(int f=0;f<50;f++)
        {
            using var region=combined.GetRegion(new Rect2I(f*64,0,64,64));using var bitmap=new Godot.Bitmap();bitmap.CreateFromImageAlpha(region,.1f);
            var polygons=bitmap.OpaqueToPolygons(new Rect2I(0,0,64,64),0);
            if(polygons.Count==0)throw new InvalidDataException("Empty SCP-999 frame.");
            var contour=polygons.OrderByDescending(p=>p.Length).First();
            foreach(bool flip in new[]{false,true})_contours[(f,flip)]=contour.Select(p=>new Vector2(flip?128-p.X*2:p.X*2,p.Y*2)).ToArray();
        }
        WindowInput+=OnInput;
    }
    public void SetLighting(float ambient,float energy,Color color)
    {
        _ambient.Color=new Color(ambient,ambient,ambient);_key.Energy=energy;_key.Color=color;
    }
    private void OnInput(InputEvent input)
    {
        if(input is InputEventMouseButton {Pressed:true} b && (b.ButtonIndex==MouseButton.Left||b.ButtonIndex==MouseButton.Right)
            && Geometry2D.IsPointInPolygon(b.Position,MousePassthroughPolygon))
            BodyPressed?.Invoke(b.ButtonIndex,WindowsDesktop.GetBounds(this).Position+(Vector2I)b.Position);
    }
    public bool ContainsDesktop(Vector2 pointer) => Visible && Geometry2D.IsPointInPolygon(pointer-(Vector2)WindowsDesktop.GetBounds(this).Position,MousePassthroughPolygon);
    public void Publish(Scp999State state)
    {
        if(!state.Visible){Hide();return;}
        int frame=state.Frame;
        if(DisplayedFrame!=frame||_flip!=state.Flipped||_hot!=state.Overheated)
        {
            DisplayedFrame=frame;_flip=state.Flipped;_body.Frame=frame;_outline.Frame=frame;_body.FlipH=_flip;_outline.FlipH=_flip;
            _hot=state.Overheated;
            _body.Scale=_outline.Scale=new(2,_hot?1.5f:2);
            _body.Position=_outline.Position=new(64,_hot?76:64);
            MousePassthroughPolygon=_hot?_contours[(frame,_flip)].Select(p=>new Vector2(p.X,112+(p.Y-112)*.75f)).ToArray():_contours[(frame,_flip)];
        }
        var p=WindowsDesktop.ToGodotPosition((Vector2I)state.Position.Round());if(Position!=p)Position=p;
        if(!Visible)Show();
    }
    public override void _ExitTree(){WindowInput-=OnInput;BodyPressed=null;}
}
