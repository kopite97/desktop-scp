using Godot;
using System;
using System.Collections.Generic;

namespace SiteDesktop.Desktop;

public readonly record struct DesktopScreen(int Index, Rect2I Bounds, Rect2I Usable, int Dpi);

public static class DesktopGeometry
{
    // Positions, native window sizes and input polygons use physical desktop pixels.
    // DPI is observed metadata, not a second multiplier on these coordinates.
    public static Vector2I ToLocal(Vector2I desktop, Vector2I windowOrigin) => desktop - windowOrigin;

    public static Vector2I Constrain(Vector2I position, Vector2I size, IReadOnlyList<DesktopScreen> screens)
    {
        if (screens.Count == 0)
            throw new ArgumentException("At least one display is required.", nameof(screens));
        Rect2I best = screens[0].Usable;
        double bestDistance = double.MaxValue;
        Vector2I center = position + size / 2;
        foreach (var screen in screens)
        {
            var area = screen.Usable;
            var nearest = new Vector2I(Math.Clamp(center.X, area.Position.X, area.End.X - 1),
                Math.Clamp(center.Y, area.Position.Y, area.End.Y - 1));
            double distance = ((Vector2)center - nearest).LengthSquared();
            if (distance < bestDistance) { best = area; bestDistance = distance; }
        }
        return new Vector2I(
            Math.Clamp(position.X, best.Position.X, Math.Max(best.Position.X, best.End.X - size.X)),
            Math.Clamp(position.Y, best.Position.Y, Math.Max(best.Position.Y, best.End.Y - size.Y)));
    }
}
