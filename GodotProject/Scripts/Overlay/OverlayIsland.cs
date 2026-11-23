using Godot;
using SiteDesktop.Desktop;
using SiteDesktop.Platform.Windows;
using System;

namespace SiteDesktop.Overlay;

public partial class OverlayIsland : Window
{
    // One authoritative contour for drawing and the OS input region. No invisible rectangle.
    public static Vector2[] Contour => new Vector2[] {
        new(24, 8), new(104, 8), new(120, 24), new(120, 104),
        new(104, 120), new(24, 120), new(8, 104), new(8, 24) };
    public OverlayToken Token { get; private set; } = null!;
    public event Action<int, Vector2I>? Pressed;
    private IslandArt? _art;

    public void Configure(OverlayToken token)
    {
        Token = token;
        Title = $"SCP Overlay {token.Id + 1}";
        Visible = false;
        ForceNative = true;
        Transient = false;
        Borderless = true;
        Unresizable = true;
        AlwaysOnTop = true;
        Unfocusable = true;
        Transparent = true;
        TransparentBg = true;
        InitialPosition = WindowInitialPosition.Absolute;
        ContentScaleMode = ContentScaleModeEnum.Disabled;
        Size = OverlayState.TokenSize;
        Position = WindowsDesktop.ToGodotPosition(token.Position);
        MousePassthroughPolygon = Contour;
    }

    public override void _Ready()
    {
        _art = new IslandArt { Token = Token };
        AddChild(_art);
        WindowInput += OnWindowInput;
    }

    private void OnWindowInput(InputEvent input)
    {
        if (input is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } button
            && Geometry2D.IsPointInPolygon(button.Position, Contour))
            Pressed?.Invoke(Token.Id, WindowsDesktop.GetBounds(this).Position + (Vector2I)button.Position);
    }

    public void Publish(bool visible)
    {
        if (!visible) { Hide(); return; }
        if (!Visible) Show();
        var position = WindowsDesktop.ToGodotPosition(Token.Position);
        if (Position != position) Position = position;
        _art?.QueueRedraw();
    }

    public override void _ExitTree() { WindowInput -= OnWindowInput; Pressed = null; }
}

internal partial class IslandArt : Node2D
{
    public OverlayToken Token { get; set; } = null!;
    public override void _Draw()
    {
        Color color = Token.Id == 0 ? new Color(1f, 0.48f, 0.12f, 0.88f) : new Color(0.16f, 0.80f, 0.85f, 0.88f);
        DrawColoredPolygon(OverlayIsland.Contour, color);
        var outline = new Vector2[] { new(24,8), new(104,8), new(120,24), new(120,104), new(104,120), new(24,120), new(8,104), new(8,24), new(24,8) };
        DrawPolyline(outline, new Color(0.04f, 0.05f, 0.07f), 2);
        DrawRect(new Rect2(34, 38, 12, 16), Colors.White);
        DrawRect(new Rect2(82, 38, 12, 16), Colors.White);
        DrawString(ThemeDB.FallbackFont, new Vector2(24, 86), $"{Token.Id + 1} : {Token.PressCount}", fontSize: 22, modulate: Colors.White);
    }
}
