namespace SiteDesktop.Simulation;

// Prototype tuning, not final product balance.
public sealed record PlagueSettings
{
    public double SpawnInterval { get; init; } = 6;
    public int MaxData { get; init; } = 6;
    public int MaxZombies { get; init; } = 8;
    public double DataLifetime { get; init; } = 90;
    public double ZombieLifetime { get; init; } = 45;
    public double TreatmentSeconds { get; init; } = 3;
    public double TargetTimeout { get; init; } = 12;
    public double RetrySeconds { get; init; } = 2;
    public float DoctorSpeed { get; init; } = 80;
    public float ZombieSpeed { get; init; } = 40;
    public float TreatmentRange { get; init; } = 44;
    public double MutationSeconds { get; init; } = .8;
}
