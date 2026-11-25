using SiteDesktop.Simulation;

namespace SiteDesktop.App.Scps;

public enum ActivityReason { Enabled, Disabled, Lockdown, Resume }

public interface IScpModule
{
    string Status { get; }
    float Stress { get; }
    void SetActivity(bool active,ActivityReason reason);
    void ApplyReactions(SystemOptions system,double delta);
    void Reset();
    void Shutdown();
}
