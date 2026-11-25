namespace SiteDesktop.Companions;

// Tunable prototype values in physical desktop pixels and seconds, not settled design values.
public sealed record Scp999Settings
{
    public float Speed { get; init; } = 160;
    public float FollowRadius { get; init; } = 420;
    public float StopDistance { get; init; } = 36;
    public float PetRadius { get; init; } = 240;
    public float ClickRelief { get; init; } = 3;
    public float RubReliefPerSecond { get; init; } = 12;
    public double ReactionCooldown { get; init; } = .65;
    public int MaxReactions { get; init; } = 4;
    public int MaxTrails { get; init; } = 12;
    public double TrailInterval { get; init; } = .30;
    public float TrailSpacing { get; init; } = 48;
    public double TrailHoldSeconds { get; init; } = 1.8;
}
