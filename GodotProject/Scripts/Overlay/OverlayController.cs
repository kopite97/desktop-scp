using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Platform.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SiteDesktop.Overlay;

public partial class OverlayController : Node
{
    public OverlayState? State { get; private set; }
    public IReadOnlyList<OverlayIsland> Islands => _islands;
    private readonly List<OverlayIsland> _islands = new();
    private double _screenTimer;
    private bool _stopped;

    public static DesktopScreen[] ReadScreens() => Enumerable.Range(0, DisplayServer.GetScreenCount())
        .Select(i => new DesktopScreen(i,
            new Rect2I(WindowsDesktop.ToDesktopPosition(DisplayServer.ScreenGetPosition(i)), DisplayServer.ScreenGetSize(i)),
            new Rect2I(WindowsDesktop.ToDesktopPosition(DisplayServer.ScreenGetUsableRect(i).Position), DisplayServer.ScreenGetUsableRect(i).Size),
            DisplayServer.ScreenGetDpi(i))).ToArray();

    public override void _Ready()
    {
        if (!OperatingSystem.IsWindows() || DisplayServer.GetName() == "headless") return;
        var screens = ReadScreens();
        if (screens.Length == 0) return;
        State = new OverlayState(screens);
        foreach (var token in State.Tokens)
        {
            var island = new OverlayIsland();
            island.Configure(token);
            island.Pressed += BeginDrag;
            AddChild(island);
            _islands.Add(island);
        }
        Publish();
        GD.Print("OVERLAY_STARTED " + Describe());
    }

    private void BeginDrag(int id, Vector2I point)
    {
        if (State is null) return;
        State.BeginDrag(id, point);
        Publish();
        GD.Print($"OVERLAY_PRESS id={id} count={State.Tokens[id].PressCount} desktop={point}");
    }

    public override void _Process(double delta)
    {
        if (State is null || _stopped) return;
        if ((_screenTimer += delta) >= 0.5)
        {
            _screenTimer = 0;
            var screens = ReadScreens();
            if (!State.Screens.SequenceEqual(screens))
            {
                State.ReplaceScreens(screens);
                Publish();
                GD.Print("OVERLAY_DISPLAYS_CHANGED " + Describe());
            }
        }
        if (State.DraggingId is int id)
        {
            if (WindowsDesktop.TryGetPointer(out var point, out bool pressed)) State.UpdateDrag(point, pressed);
            else State.CancelDrag();
            Publish();
            if (State.DraggingId is null) GD.Print($"OVERLAY_DRAG_END id={id} position={State.Tokens[id].Position}");
        }
    }

    public void SetVisible(bool visible)
    {
        if (State is null || _stopped) return;
        State.SetVisible(visible);
        Publish();
        GD.Print(visible ? "OVERLAY_SHOWN" : "OVERLAY_HIDDEN");
    }

    public void ResetPositions()
    {
        if (State is null || _stopped) return;
        State.ResetPositions();
        Publish();
        GD.Print("OVERLAY_RESET " + Describe());
    }

    private void Publish() { foreach (var island in _islands) island.Publish(State!.Visible); }

    public void Shutdown()
    {
        if (_stopped) return;
        _stopped = true;
        State?.SetVisible(false);
        foreach (var island in _islands) { island.Pressed -= BeginDrag; island.Hide(); island.QueueFree(); }
        _islands.Clear();
        GD.Print("OVERLAY_STOPPED");
    }

    public string Describe() => JsonSerializer.Serialize(new {
        screens = State?.Screens.Select(s => new { s.Index, x=s.Bounds.Position.X, y=s.Bounds.Position.Y,
            width=s.Bounds.Size.X, height=s.Bounds.Size.Y, dpi=s.Dpi,
            usable=new[]{s.Usable.Position.X,s.Usable.Position.Y,s.Usable.Size.X,s.Usable.Size.Y} }),
        tokens = _islands.Select(w => new { id=w.Token.Id, x=w.Token.Position.X, y=w.Token.Position.Y,
            godotX=w.Position.X, godotY=w.Position.Y, width=w.Size.X, height=w.Size.Y, dpi=WindowsDesktop.GetDpi(w) }) });

    public override void _ExitTree() => Shutdown();
}
