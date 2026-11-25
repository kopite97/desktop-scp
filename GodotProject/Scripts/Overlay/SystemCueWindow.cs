using Godot;
using SiteDesktop.Platform.Windows;

namespace SiteDesktop.Overlay;

// Decorative only; native layered transparency prevents input capture across processes.
public partial class SystemCueWindow : Window
{
    public const double LockdownCueSeconds = 1.5;
    public bool Shutter { get; set; }
    public float Age { get; set; }
    private SystemCueGlyph _glyph=null!;
    public SystemCueWindow()
    {Title="Site system cues";Visible=false;ForceNative=true;Transient=false;Borderless=true;AlwaysOnTop=true;Unfocusable=true;Transparent=true;TransparentBg=true;MousePassthrough=true;Unresizable=true;InitialPosition=WindowInitialPosition.Absolute;}
    public override void _Ready(){_glyph=new();AddChild(_glyph);}
    public void Publish(Rect2I area,bool shutter,float age,bool noisy)
    {
        Shutter=shutter;Age=age;
        Size=shutter?new Vector2I(440,112):new Vector2I(96,96);
        Position=WindowsDesktop.ToGodotPosition(shutter?area.Position+new Vector2I((area.Size.X-Size.X)/2,24):area.Position+new Vector2I(area.Size.X-100,4));
        _glyph.Extent=Size;_glyph.Shutter=shutter;_glyph.Age=age;_glyph.Noisy=noisy;_glyph.QueueRedraw();
        if(!Visible)Show();WindowsDesktop.MakeDecorativeClickThrough(this);
    }
}
public partial class SystemCueGlyph : Node2D
{
    public Vector2I Extent;public bool Shutter,Noisy;public float Age;
    private readonly StyleBoxFlat _panel = new()
    {
        BgColor=new Color(.055f,.075f,.10f,.96f),
        BorderColor=new Color(.35f,.65f,.8f,.8f),
        BorderWidthLeft=1,BorderWidthRight=1,BorderWidthTop=1,BorderWidthBottom=1,
        CornerRadiusTopLeft=14,CornerRadiusTopRight=14,CornerRadiusBottomLeft=14,CornerRadiusBottomRight=14
    };
    public override void _Draw()
    {
        if(Shutter)
        {
            float alpha=Mathf.Min(Mathf.Clamp(Age/.12f,0,1),Mathf.Clamp(((float)SystemCueWindow.LockdownCueSeconds-Age)/.3f,0,1));
            Modulate=new Color(1,1,1,alpha);
            DrawStyleBox(_panel,new Rect2(0,0,Extent.X,Extent.Y));
            var accent=new Color(.55f,.85f,1);
            DrawArc(new(38,45),10,Mathf.Pi,Mathf.Tau,20,accent,3,true);
            DrawRect(new Rect2(25,44,26,23),accent,false,3);
            DrawCircle(new(38,54),2,accent);DrawLine(new(38,54),new(38,60),accent,2);
            DrawString(ThemeDB.FallbackFont,new(70,31),"LOCKDOWN",HorizontalAlignment.Left,-1,16,accent);
            DrawString(ThemeDB.FallbackFont,new(70,62),"SCP 활동 일시 정지",HorizontalAlignment.Left,-1,24,Colors.White);
            DrawString(ThemeDB.FallbackFont,new(70,89),"Ctrl+Shift+L 또는 트레이 메뉴로 재개",HorizontalAlignment.Left,-1,16,new Color(.78f,.83f,.88f));
            return;
        }
        Modulate=Colors.White;
        for(int i=0;i<7;i++)
        {float phase=(Age*.6f+i/7f)%1;DrawRect(new(12+i*10,82-phase*70,3,5),Noisy?new Color(1,.8f,.2f,1-phase):new Color(1,.35f,.08f,1-phase));}
    }
    public override void _ExitTree()=>_panel.Dispose();
}
