using Godot;
namespace SiteDesktop.Platforms;
public enum MovementStatus { Moving, Arrived, Blocked, Unreachable, Cancelled }
public sealed record MovementGoal(Vector2 Position,string? SupportId,float Tolerance=8);
public sealed record MovementSettings(double StuckSeconds=1.5,double TimeoutSeconds=12,bool DistanceBudget=false);
