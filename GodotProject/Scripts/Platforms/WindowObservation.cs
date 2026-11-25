using Godot;

namespace SiteDesktop.Platforms;

// Immutable observation, ordered front-to-back. No window titles or document content.
public sealed record WindowObservation(string Id, Rect2I Bounds, bool Visible = true,
    bool Minimized = false, bool Cloaked = false, bool CanSupport = true, bool Blocks = true);
public sealed record PlatformSurface(string Id, float Left, float Right, float Y, Vector2 WindowOrigin, bool Floor = false)
{
    public bool Contains(float x, float halfWidth) => x - halfWidth >= Left && x + halfWidth <= Right;
}
public enum PlatformMotion { Grounded, Takeoff, Falling, Landing, Balancing, Dragging }
public sealed record PlatformSettings
{
    public float Speed { get; init; } = 160;
    public float Gravity { get; init; } = 1800;
    public float JumpSpeed { get; init; } = 780;
    public float HalfWidth { get; init; } = 24;
    public float Height { get; init; } = 96;
    public float MaxJumpRise { get; init; } = 160;
    public float MaxJumpGap { get; init; } = 260;
    public float MaxCarryDelta { get; init; } = 200;
}
