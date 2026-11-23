using Godot;
using System.Collections.Generic;

namespace SiteDesktop.Desktop;

public sealed class OverlayToken
{
    public int Id { get; }
    public Vector2I Position { get; internal set; }
    public int PressCount { get; internal set; }
    public OverlayToken(int id, Vector2I position) { Id = id; Position = position; }
}

public sealed class OverlayState
{
    public static readonly Vector2I TokenSize = new(128, 128);
    public IReadOnlyList<DesktopScreen> Screens { get; private set; }
    public IReadOnlyList<OverlayToken> Tokens { get; }
    public bool Visible { get; private set; } = true;
    public int? DraggingId { get; private set; }
    private Vector2I _dragOffset;

    public OverlayState(IReadOnlyList<DesktopScreen> screens)
    {
        Screens = screens;
        Tokens = new[] { new OverlayToken(0, Vector2I.Zero), new OverlayToken(1, Vector2I.Zero) };
        ResetPositions();
    }

    public void ResetPositions()
    {
        CancelDrag();
        var origin = Screens[0].Usable.Position;
        foreach (var token in Tokens)
            token.Position = DesktopGeometry.Constrain(origin + new Vector2I(240 + token.Id * 240, 430), TokenSize, Screens);
    }

    public void SetVisible(bool visible) { Visible = visible; CancelDrag(); }
    public void CancelDrag() => DraggingId = null;
    public void BeginDrag(int id, Vector2I pointer)
    {
        if (!Visible || id < 0 || id >= Tokens.Count) return;
        DraggingId = id;
        _dragOffset = pointer - Tokens[id].Position;
        Tokens[id].PressCount++;
    }

    public void UpdateDrag(Vector2I pointer, bool pressed)
    {
        if (!Visible) { CancelDrag(); return; }
        if (DraggingId is int id)
            Tokens[id].Position = DesktopGeometry.Constrain(pointer - _dragOffset, TokenSize, Screens);
        // A complete gesture can arrive between frames; retain the final pointer position.
        if (!pressed) CancelDrag();
    }

    public void ReplaceScreens(IReadOnlyList<DesktopScreen> screens)
    {
        if (screens.Count == 0) return;
        Screens = screens;
        CancelDrag();
        foreach (var token in Tokens)
            token.Position = DesktopGeometry.Constrain(token.Position, TokenSize, Screens);
    }
}
