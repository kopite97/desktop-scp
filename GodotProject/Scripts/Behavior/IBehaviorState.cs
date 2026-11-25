namespace SiteDesktop.Behavior;

public interface IBehaviorState
{
    string Name { get; }
    void Enter();
    void Tick(double delta);
    void Exit();
}
