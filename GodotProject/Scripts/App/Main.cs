using Godot;
using SiteDesktop.Overlay;

namespace SiteDesktop.App;

public partial class Main : Control
{
    private Button? _exitButton;
    private Button? _visibilityButton;
    private Button? _resetButton;
    public OverlayController Overlays { get; private set; } = null!;

    public override void _Ready()
    {
        GetNode<Label>("Content/RuntimeStatus").Text = "C# runtime ready";
        _exitButton = GetNode<Button>("Content/Exit");
        _exitButton.Pressed += RequestExit;
        _visibilityButton = GetNode<Button>("Content/Visibility");
        _visibilityButton.Pressed += ToggleOverlays;
        _resetButton = GetNode<Button>("Content/Reset");
        _resetButton.Pressed += ResetOverlays;
        GetTree().AutoAcceptQuit = false;
        GetWindow().CloseRequested += RequestExit;
        Overlays = new OverlayController();
        AddChild(Overlays);
        GD.Print("SITE_DESKTOP_READY");
    }

    private void ToggleOverlays()
    {
        bool show = !(Overlays.State?.Visible ?? false);
        Overlays.SetVisible(show);
        _visibilityButton!.Text = show ? "Hide overlays" : "Show overlays";
    }

    private void ResetOverlays() => Overlays.ResetPositions();

    public void RequestExit()
    {
        GD.Print("SITE_DESKTOP_EXIT_REQUESTED");
        Overlays.Shutdown();
        GetTree().Quit();
    }

    public override void _ExitTree()
    {
        if (_exitButton is not null)
            _exitButton.Pressed -= RequestExit;
        if (_visibilityButton is not null) _visibilityButton.Pressed -= ToggleOverlays;
        if (_resetButton is not null) _resetButton.Pressed -= ResetOverlays;
        GetWindow().CloseRequested -= RequestExit;
        GD.Print("SITE_DESKTOP_STOPPED");
    }
}
