using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;

namespace SiteDesktop.App;

public partial class PlatformDesktop : Control
{
    public PlatformController Controller { get; private set; } = null!;
    private Label _status = null!;
    public override void _Ready()
    {
        var window = GetWindow(); var screen = DesktopPlacement.Select();
        window.Title = "SCP — Window platforms"; window.Size = new(520, 350);
        window.Position = WindowsDesktop.ToGodotPosition(screen.Usable.Position + new Vector2I(30, 50));
        window.CloseRequested += Exit; GetTree().AutoAcceptQuit = false;
        var box = new VBoxContainer { Position = new(24, 24), Size = new(472, 300) }; AddChild(box);
        box.AddChild(new Label { Text = "WINDOW PLATFORMS", ThemeTypeVariation = "HeaderSmall" });
        box.AddChild(new Label { Text = "Right-drag the marker above a window and release.\nLeft-click it to jump to a reachable neighboring ledge.\nWalking near an edge can jump automatically." });
        _status = new Label(); box.AddChild(_status);
        var row = new HBoxContainer(); box.AddChild(row);
        AddButton(row, "← Walk", () => Controller.World.Walk(-1));
        AddButton(row, "Stop", () => Controller.World.Walk(0));
        AddButton(row, "Walk →", () => Controller.World.Walk(1));
        AddButton(row, "Jump", () => Controller.Jump());
        AddButton(box, "Hide / Show", () => Controller.SetVisible(!Controller.World.Visible));
        AddButton(box, "Reset position", () => { Controller.World.Reset(); Controller.Publish(); });
        AddButton(box, "Exit", Exit);
        Controller = new(); AddChild(Controller);
        GD.Print("PLATFORM_DESKTOP_READY");
    }
    private static void AddButton(Node parent, string text, System.Action action)
    { var button = new Button { Text = text }; button.Pressed += action; parent.AddChild(button); }
    public override void _Process(double delta)
    {
        if (Controller == null) return;
        var world = Controller.World;
        _status.Text = $"{world.Body.Motion}  |  Ledges: {world.Surfaces.Count}\n{(world.Visible ? "Active" : "Paused")}  •  Position {world.Body.Foot.Round()}";
    }
    public void Exit() { Controller.Shutdown(); GetTree().Quit(); }
    public override void _ExitTree() { GetWindow().CloseRequested -= Exit; }
}
