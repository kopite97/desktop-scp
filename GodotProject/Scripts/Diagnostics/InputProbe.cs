using Godot;
using System.Text.Json;

namespace SiteDesktop.Diagnostics;

// Separate process target: only this test window receives/logs clicks, never a user's app.
public partial class InputProbe : Node2D
{
    private int _downCount;
    private int _upCount;
    private Vector2 _last;

    public override void _Ready()
    {
        var window = GetWindow();
        window.Title = "SCP Input Receiver";
        window.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
        window.Size = new Vector2I(1000, 740);
        window.Position = DisplayServer.ScreenGetUsableRect(0).Position + new Vector2I(100, 120);
        GD.Print($"INPUT_PROBE_READY position={window.Position} size={window.Size}");
    }

    public override void _Input(InputEvent input)
    {
        if (input is not InputEventMouseButton { ButtonIndex: MouseButton.Left } button) return;
        if (button.Pressed) _downCount++; else _upCount++;
        _last = button.Position;
        GD.Print("INPUT_PROBE " + JsonSerializer.Serialize(new { down=_downCount, up=_upCount,
            pressed=button.Pressed, x=_last.X, y=_last.Y }));
        QueueRedraw();
    }

    public override void _Draw()
    {
        for (int y=0; y<740; y+=40)
            for (int x=0; x<1000; x+=40)
                DrawRect(new Rect2(x,y,40,40), ((x+y)/40)%2 == 0 ? new Color(0.18f,0.23f,0.28f) : new Color(0.25f,0.31f,0.36f));
        DrawString(ThemeDB.FallbackFont, new Vector2(24,44), "INPUT RECEIVER — separate process", fontSize: 28);
        DrawString(ThemeDB.FallbackFont, new Vector2(24,86), $"Down: {_downCount}   Up: {_upCount}   Last: {_last}", fontSize: 24);
        DrawString(ThemeDB.FallbackFont, new Vector2(24,130), "Click gaps / cut corners; body clicks must not arrive here.", fontSize: 22);
        DrawString(ThemeDB.FallbackFont, new Vector2(24,690), "Close this window after validation.", fontSize: 20);
    }
}
