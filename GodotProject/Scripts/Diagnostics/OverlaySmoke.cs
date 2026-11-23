using Godot;
using SiteDesktop.App;
using SiteDesktop.Desktop;
using SiteDesktop.Platform.Windows;
using System;

namespace SiteDesktop.Diagnostics;

public partial class OverlaySmoke : Node
{
    private Main? _main;
    private int _frames;
    private int _checks;
    private bool _failed;

    public override void _Ready()
    {
        try
        {
            var screens = new[] {
                new DesktopScreen(0, new Rect2I(-1920,-200,1920,1080),new Rect2I(-1920,-200,1920,1040),96),
                new DesktopScreen(1, new Rect2I(0,0,2560,1440),new Rect2I(0,0,2560,1400),144) };
            Require(DesktopGeometry.ToLocal(new Vector2I(-1800,-100),new Vector2I(-1920,-200)) == new Vector2I(120,100), "negative-origin conversion");
            Require(DesktopGeometry.Constrain(new Vector2I(-30,200),OverlayState.TokenSize,screens) == new Vector2I(0,200), "monitor boundary clamp");
            Require(DesktopGeometry.Constrain(new Vector2I(2500,1390),OverlayState.TokenSize,screens) == new Vector2I(2432,1272), "work-area clamp");
            var state = new OverlayState(screens);
            var original = state.Tokens[0].Position;
            state.BeginDrag(0,original + new Vector2I(20,20));
            state.UpdateDrag(new Vector2I(150,250),true);
            Require(state.Tokens[0].Position == new Vector2I(130,230), "cross-monitor physical pixels, no DPI double scaling");
            state.SetVisible(false);
            var hidden = state.Tokens[0].Position;
            state.UpdateDrag(new Vector2I(900,900),true);
            Require(state.DraggingId is null && state.Tokens[0].Position == hidden, "hide cancels drag");
            state.BeginDrag(1,new Vector2I(900,900));
            Require(state.Tokens[1].PressCount == 0, "hidden input ignored");
            state.SetVisible(true);
            Require(state.Tokens[0].Position == hidden && state.Tokens[0].PressCount == 1, "restore preserves state");
            state.ReplaceScreens(new[] { screens[0] });
            Require(screens[0].Usable.Encloses(new Rect2I(state.Tokens[0].Position,OverlayState.TokenSize)), "removed monitor recovery");
            state.BeginDrag(0,state.Tokens[0].Position);
            state.UpdateDrag(new Vector2I(999,999),false);
            Require(state.DraggingId is null, "release outside cancels capture");
            state.BeginDrag(0,state.Tokens[0].Position + new Vector2I(10,10));
            state.UpdateDrag(new Vector2I(-1000,300),false);
            Require(state.DraggingId is null && state.Tokens[0].Position == new Vector2I(-1010,290), "fast drag retains release position");
            _main = GD.Load<PackedScene>("res://Scenes/App/Main.tscn").Instantiate<Main>();
            AddChild(_main);
        }
        catch (Exception error) { Fail(error); }
    }

    public override void _Process(double delta)
    {
        if (_failed || _main is null) return;
        try
        {
            _frames++;
            if (_frames == 10 && DisplayServer.GetName() != "headless")
            {
                Require(_main.Overlays.Islands.Count == 2, "two native islands");
                foreach (var island in _main.Overlays.Islands)
                {
                    Require(!island.IsEmbedded() && island.Visible && island.Transparent && island.TransparentBg, "native transparent window");
                    Require(WindowsDesktop.GetBounds(island) == new Rect2I(island.Token.Position,OverlayState.TokenSize), "Win32 and Godot physical coordinate agreement");
                    Require(island.MousePassthroughPolygon.Length == 8, "input contour installed");
                }
                _main.Overlays.SetVisible(false);
            }
            if (_frames == 15 && DisplayServer.GetName() != "headless")
            {
                foreach (var island in _main.Overlays.Islands) Require(!island.Visible, "native window hidden");
                _main.Overlays.SetVisible(true);
            }
            bool native = DisplayServer.GetName() != "headless";
            if (_frames == 25 && native)
                foreach (var island in _main.Overlays.Islands)
                    Require(island.Visible && WindowsDesktop.GetBounds(island).Position == island.Token.Position, "restore location");
            if (native && _frames >= 25)
            {
                var state = _main.Overlays.State!;
                int monitor = (_frames - 25) / 5;
                if (monitor < state.Screens.Count)
                {
                    if ((_frames - 25) % 5 == 0)
                    {
                        // Exercise the same state commands, then publish to a real native window.
                        state.BeginDrag(0,state.Tokens[0].Position);
                        state.UpdateDrag(state.Screens[monitor].Usable.Position + new Vector2I(64,64),false);
                        _main.Overlays.SetVisible(true);
                    }
                    if ((_frames - 25) % 5 == 4)
                    {
                        var bounds = WindowsDesktop.GetBounds(_main.Overlays.Islands[0]);
                        Require(bounds.Position == state.Tokens[0].Position && state.Screens[monitor].Usable.Encloses(bounds),
                            $"native monitor {monitor} placement at {bounds.Position}");
                    }
                    return;
                }
            }
            if (_frames < 25) return;
            GD.Print($"OVERLAY_SMOKE_PASS checks={_checks} display={DisplayServer.GetName()}");
            SetProcess(false);
            _main.RequestExit();
        }
        catch (Exception error) { Fail(error); }
    }

    private void Require(bool condition,string name)
    {
        if (!condition) throw new InvalidOperationException(name);
        _checks++;
        GD.Print("OVERLAY_CHECK_PASS " + name);
    }

    private void Fail(Exception error)
    {
        _failed = true;
        GD.PushError("OVERLAY_SMOKE_FAILED " + error);
        _main?.Overlays.Shutdown();
        GetTree().Quit(1);
    }
}
