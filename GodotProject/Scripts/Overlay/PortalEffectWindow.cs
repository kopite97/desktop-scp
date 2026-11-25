using Godot;
using SiteDesktop.Platform.Windows;
using System;

namespace SiteDesktop.Overlay;
public partial class PortalEffectWindow : Window
{
    private PortalGlyph _glyph=null!;
    public PortalEffectWindow()
    {
        Title="SCP portal";Visible=false;ForceNative=true;Transient=false;Borderless=true;Unresizable=true;AlwaysOnTop=true;
        Unfocusable=true;Transparent=true;TransparentBg=true;MousePassthrough=true;
        InitialPosition=WindowInitialPosition.Absolute;ContentScaleMode=ContentScaleModeEnum.Disabled;Size=new(160,160);
    }
    public override void _Ready(){_glyph=new(){Material=new CanvasItemMaterial{LightMode=CanvasItemMaterial.LightModeEnum.Unshaded}};AddChild(_glyph);}
    public void Publish(Vector2 foot,double age,bool arrival)
    {
        double local=arrival?age-.4:age;
        float open=(float)Math.Clamp(local/.16,0,1),close=(float)Math.Clamp((.6-local)/.2,0,1);
        _glyph.Amount=Math.Min(open,close);_glyph.Age=(float)age;
        if(_glyph.Amount<=0){Hide();return;}
        Position=WindowsDesktop.ToGodotPosition((Vector2I)(foot-new Vector2(80,152)).Round());_glyph.QueueRedraw();
        if(!Visible){Show();WindowsDesktop.MakeDecorativeClickThrough(this);}
    }
}
public partial class PortalGlyph : Node2D
{
    public float Amount,Age;
    public override void _Draw()
    {
        if(Amount<=.001f)return;
        var points=new Vector2[49];var center=new Vector2(80,82);
        for(int i=0;i<points.Length;i++){float a=i*MathF.Tau/(points.Length-1);points[i]=center+new Vector2(MathF.Cos(a)*34*Amount,MathF.Sin(a)*64);}
        DrawColoredPolygon(points[..^1],new Color(.03f,.09f,.12f,.88f*Amount));
        DrawPolyline(points,new Color(.15f,.62f,.68f,.25f*Amount),9,true);
        DrawPolyline(points,new Color(.35f,.92f,.9f,.9f*Amount),3,true);
        for(int i=0;i<8;i++){float a=i*MathF.Tau/8+Age*2;var p=center+new Vector2(MathF.Cos(a)*(39+4*MathF.Sin(Age*5+i))*Amount,MathF.Sin(a)*66);DrawRect(new(p-new Vector2(1,1),new Vector2(3,3)),new Color(.65f,1,1,.65f*Amount));}
    }
}
