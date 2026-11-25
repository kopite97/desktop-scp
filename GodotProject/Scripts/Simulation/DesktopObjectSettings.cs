namespace SiteDesktop.Simulation;
public record DesktopObjectSettings
{
    public double SpawnInterval { get; init; }=6;
    public int MaxData { get; init; }=6;
    public double DataLifetime { get; init; }=90;
    public ZombieSettings Zombies { get; init; }=new();
}
public sealed record ZombieSettings
{
    public int Maximum { get; init; }=8;
    public double Lifetime { get; init; }=45;
    public float Speed { get; init; }=40;
    public double MutationSeconds { get; init; }=.8;
}
