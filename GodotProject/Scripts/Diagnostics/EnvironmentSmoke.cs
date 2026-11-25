using Godot;
using SiteDesktop.App;

namespace SiteDesktop.Diagnostics;

// Loads the real entry scene and exits through the common explicit shutdown command.
public partial class EnvironmentSmoke : Node
{
    private Main? _main;
    private int _frames;

    public override void _Ready()
    {
        _main = GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();_main.PersistScpSettings=false;_main.EnableSystemOptions=false;_main.EnableStatue=false;
        AddChild(_main);
    }

    public override void _Process(double delta)
    {
        if (++_frames < 3)
            return;
        SetProcess(false);

        if (_main is null || !_main.IsNodeReady()
            || _main.GetNode<Label>("Scroll/Content/RuntimeStatus").Text != "C# runtime ready")
        {
            GD.PushError("ENVIRONMENT_SMOKE_FAILED: main scene did not initialize through C#.");
            GetTree().Quit(1);
            return;
        }

        GD.Print($"ENVIRONMENT_SMOKE_PASS frames={_frames} display={DisplayServer.GetName()} runtime={System.Environment.Version}");
        _main.RequestExit();
    }
}
