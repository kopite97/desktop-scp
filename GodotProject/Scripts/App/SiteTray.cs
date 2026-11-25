using Godot;
using System;

namespace SiteDesktop.App;

public sealed class SiteTray : IDisposable
{
    public int Indicator { get; private set; }=-1;
    public Rid Menu { get; private set; }
    public Callable ToggleCallback { get; private set; }
    private ImageTexture? _icon;
    public bool Available=>Indicator>=0;
    public SiteTray(Action toggle,Action controls,Action exit,Func<Texture2D,string,Callable,int>? register=null)
    {
        if(DisplayServer.GetName()=="headless")return;
        using var image=Image.CreateEmpty(16,16,false,Image.Format.Rgba8);image.Fill(Colors.Transparent);
        for(int y=1;y<15;y++)for(int x=1;x<15;x++)
        {float r=new Vector2(x-7.5f,y-7.5f).Length();if(r>4&&r<7||x>=7&&x<=8||y>=7&&y<=8)image.SetPixel(x,y,Colors.White);}
        _icon=ImageTexture.CreateFromImage(image);
        Menu=NativeMenu.CreateMenu();
        ToggleCallback=Callable.From<Variant>(_=>toggle());
        NativeMenu.AddItem(Menu,"Lockdown / Resume",ToggleCallback);
        NativeMenu.AddItem(Menu,"Show controls",Callable.From<Variant>(_=>controls()));
        NativeMenu.AddItem(Menu,"Exit",Callable.From<Variant>(_=>exit()));
        try
        {
            var callback=Callable.From<int,Vector2I>((button,_)=>{if(button==1)controls();});
            Indicator=register!=null?register(_icon,"SCP: Site-Desktop",callback):DisplayServer.CreateStatusIndicator(_icon,"SCP: Site-Desktop",callback);
            if(Available)DisplayServer.StatusIndicatorSetMenu(Indicator,Menu);
            else Dispose();
        }
        catch { Dispose(); throw; }
    }
    public void Update(bool locked)
    {if(Available){NativeMenu.SetItemText(Menu,0,locked?"Resume":"Lockdown");DisplayServer.StatusIndicatorSetTooltip(Indicator,locked?"SCP: Locked — Ctrl+Shift+L to resume":"SCP: Site-Desktop — Ctrl+Shift+L to lock");}}
    public void Dispose()
    {if(Indicator>=0){DisplayServer.DeleteStatusIndicator(Indicator);Indicator=-1;}if(Menu.IsValid){NativeMenu.FreeMenu(Menu);Menu=default;}_icon?.Dispose();_icon=null;}
}
