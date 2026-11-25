namespace SiteDesktop.Simulation;

// Prototype tuning, not final product balance.
public sealed record PlagueSettings
{
    public double TreatmentSeconds { get; init; } = 3;
    public double TargetTimeout { get; init; } = 12;
    public double RetrySeconds { get; init; } = 2;
    public float DoctorSpeed { get; init; } = 80;
    public float TreatmentRange { get; init; } = 44;
}
