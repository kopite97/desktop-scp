using Godot;

namespace SiteDesktop.Platforms;

public sealed class PlatformBody
{
    public Vector2 Foot { get; internal set; }
    public Vector2 Velocity { get; internal set; }
    public string? SupportId { get; internal set; }
    public PlatformMotion Motion { get; internal set; } = PlatformMotion.Falling;
    public int WalkDirection { get; internal set; }
    public bool AutoJump { get; set; } = true;
    public int Landings { get; internal set; }
    public int Jumps { get; internal set; }
    internal Vector2 SupportOrigin;
    internal float Slip;
    internal double PoseTime;
    internal double JumpCooldown;
    internal Vector2 DragOffset;
}
