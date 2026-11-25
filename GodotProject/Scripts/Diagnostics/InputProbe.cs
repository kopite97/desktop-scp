using Godot;
using System.Text.Json;
using System.IO;

namespace SiteDesktop.Diagnostics;

// Separate process target: only this test window receives/logs clicks, never a user's app.
public partial class InputProbe : Node2D
{
    private int _downCount;
    private int _upCount;
    private Vector2 _last;
    private int _keys;
    private string? _output;

    public override void _Ready()
    {
        var window = GetWindow();
        window.Title = "SCP Input Receiver";
        window.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
        window.Size = new Vector2I(1000, 740);
        window.Position = DisplayServer.ScreenGetUsableRect(0).Position + new Vector2I(100, 120);
        var args=OS.GetCmdlineUserArgs();
        for(int i=0;i<args.Length;i++) if(args[i]=="--receiver-output") _output=args[++i];
        if(_output!=null)
        {
            window.Borderless=true;
            for(int i=0;i+1<args.Length;i++)if(args[i]=="--receiver-on-top")window.AlwaysOnTop=args[i+1]=="true";
            var physical=SiteDesktop.Desktop.DesktopPlacement.Select().Usable;
            var area=new Rect2I(SiteDesktop.Platform.Windows.WindowsDesktop.ToGodotPosition(physical.Position),physical.Size);
            window.Position=area.Position;window.Size=area.Size;
            for(int i=0;i+1<args.Length;i++)if(args[i]=="--receiver-decorated"&&args[i+1]=="true")
            {window.Borderless=false;window.Position=area.Position+new Vector2I(900,160);window.Size=new(640,480);}
            SaveCounts();
        }
        GD.Print($"INPUT_PROBE_READY position={window.Position} size={window.Size}");
    }

    public override void _Input(InputEvent input)
    {
        if(input is InputEventKey {Pressed:true}){_keys++;SaveCounts();return;}
        if (input is not InputEventMouseButton { ButtonIndex: MouseButton.Left } button) return;
        if (button.Pressed) _downCount++; else _upCount++;
        _last = button.Position;
        GD.Print("INPUT_PROBE " + JsonSerializer.Serialize(new { down=_downCount, up=_upCount,
            pressed=button.Pressed, x=_last.X, y=_last.Y }));
        QueueRedraw();
        SaveCounts();
    }

    private void SaveCounts()
    {
        if(_output==null)return;
        var handle=(long)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle,GetWindow().GetWindowId());
        File.WriteAllText(_output+".tmp",JsonSerializer.Serialize(new{down=_downCount,up=_upCount,keys=_keys,handle}));
        File.Move(_output+".tmp",_output,true);
    }

    public override void _Draw()
    {
        for (int y=0; y<GetWindow().Size.Y; y+=40)
            for (int x=0; x<GetWindow().Size.X; x+=40)
                DrawRect(new Rect2(x,y,40,40), ((x+y)/40)%2 == 0 ? new Color(0.18f,0.23f,0.28f) : new Color(0.25f,0.31f,0.36f));
        DrawString(ThemeDB.FallbackFont, new Vector2(24,44), "INPUT RECEIVER — separate process", fontSize: 28);
        DrawString(ThemeDB.FallbackFont, new Vector2(24,86), $"Down: {_downCount}   Up: {_upCount}   Last: {_last}", fontSize: 24);
        DrawString(ThemeDB.FallbackFont, new Vector2(24,130), "Click gaps / cut corners; body clicks must not arrive here.", fontSize: 22);
        DrawString(ThemeDB.FallbackFont, new Vector2(24,690), "Close this window after validation.", fontSize: 20);
    }
}
