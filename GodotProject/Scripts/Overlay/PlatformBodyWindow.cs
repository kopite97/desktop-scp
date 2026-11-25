using Godot;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Platforms;
using System;

namespace SiteDesktop.Overlay;

// A neutral physics marker, not a new SCP AI or new art revision.
public partial class PlatformBodyWindow : Window
{
    public event Action<Vector2>? DragPressed;
    public event Action? JumpPressed;
    private Polygon2D _shape = null!;
    private static readonly Vector2[] Shape = { new(8, 8), new(40, 8), new(48, 24), new(48, 96), new(0, 96), new(0, 24) };
    public PlatformBodyWindow()
    {
        Title = "SCP physics body"; Visible = false; ForceNative = true; Transient = false;
        Borderless = true; Unresizable = true; AlwaysOnTop = true; Unfocusable = true;
        Transparent = true; TransparentBg = true; ContentScaleMode = ContentScaleModeEnum.Disabled;
        InitialPosition = WindowInitialPosition.Absolute; Size = new(48, 96); MousePassthroughPolygon = Shape;
    }
    public override void _Ready()
    {
        _shape = new Polygon2D { Polygon = Shape, Color = new Color("5ac8da") }; AddChild(_shape);
        var eyes = new Label { Text = "• •", Position = new(10, 22), Modulate = new Color("203646") }; AddChild(eyes);
        WindowInput += Input;
    }
    private void Input(InputEvent input)
    {
        if (input is not InputEventMouseButton { Pressed: true } button) return;
        if (button.ButtonIndex == MouseButton.Right) DragPressed?.Invoke(WindowsDesktop.GetBounds(this).Position + button.Position);
        if (button.ButtonIndex == MouseButton.Left) JumpPressed?.Invoke();
    }
    public void Publish(PlatformWorld world)
    {
        if (!world.Visible || world.Stopped) { Hide(); return; }
        Position = WindowsDesktop.ToGodotPosition((Vector2I)(world.Body.Foot - new Vector2(24, 96)).Round());
        _shape.Color = world.Body.Motion switch
        {
            PlatformMotion.Takeoff => new Color("f5cb65"), PlatformMotion.Falling => new Color("b39bdf"),
            PlatformMotion.Balancing => new Color("ef9967"), _ => new Color("5ac8da")
        };
        if (!Visible) Show();
    }
    public override void _ExitTree() { WindowInput -= Input; DragPressed = null; JumpPressed = null; }
}
