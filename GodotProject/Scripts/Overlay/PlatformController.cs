using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Platforms;
using System;
using System.Linq;

namespace SiteDesktop.Overlay;

public partial class PlatformController : Node
{
    public PlatformWorld World { get; private set; } = null!;
    public WindowsWindowObserver Observer { get; } = new();
    public PlatformBodyWindow? BodyWindow { get; private set; }
    public bool Automatic { get; set; } = true;
    private double _observationTime, _screenTime;
    private bool _native, _stopped;
    public override void _Ready()
    {
        var screen = DesktopPlacement.Select();
        World = new(new[] { screen });
        _native = OperatingSystem.IsWindows() && DisplayServer.GetName() != "headless";
        if (!_native) return;
        BodyWindow = new(); BodyWindow.DragPressed += World.BeginDrag; BodyWindow.JumpPressed += Jump;
        AddChild(BodyWindow); Refresh(); Publish();
        GD.Print($"PLATFORM_STARTED monitor={screen.Index} origin={screen.Bounds.Position}");
    }
    public void Jump() { World.JumpToAdjacent(World.Body.WalkDirection); Publish(); }
    public void Refresh()
    {
        if (!_native || _stopped || !World.Visible) return;
        World.Observe(Observer.Read());
        if (BodyWindow is { Visible: true }) WindowsDesktop.RaiseWithoutFocus(BodyWindow);
    }
    public override void _Process(double delta)
    {
        if (_stopped || !Automatic || !World.Visible) return;
        if (_native && (_screenTime += delta) >= .5)
        {
            _screenTime = 0;
            var screens = OverlayController.ReadScreens();
            if (screens.Length > 0)
            {
                DesktopScreen selected;
                try { selected = DesktopPlacement.Select(); } catch { selected = screens[0]; }
                if (selected != World.Screens[0]) { World.ReplaceScreens(new[] { selected }); Refresh(); }
            }
        }
        if ((_observationTime += delta) >= .05) { _observationTime = 0; Refresh(); }
        if (_native && World.Body.Motion == PlatformMotion.Dragging)
        {
            if (WindowsDesktop.TryGetPointer(out var point, out _) && WindowsDesktop.RightPressed) World.Drag(point);
            else World.EndDrag();
        }
        Advance(delta);
    }
    public void Advance(double seconds) { if (_stopped) return; World.Tick(seconds); Publish(); }
    public void Publish() => BodyWindow?.Publish(World);
    public void SetVisible(bool visible)
    {
        if (_stopped) return;
        World.SetVisible(visible); _observationTime = 0;
        if (visible) Refresh(); Publish();
    }
    public void Shutdown()
    {
        if (_stopped) return; _stopped = true; World.Stop();
        if (BodyWindow != null)
        {
            BodyWindow.DragPressed -= World.BeginDrag; BodyWindow.JumpPressed -= Jump;
            BodyWindow.Hide(); BodyWindow.QueueFree(); BodyWindow = null;
        }
        GD.Print("PLATFORM_STOPPED");
    }
    public override void _ExitTree() => Shutdown();
}
