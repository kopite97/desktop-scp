using SiteDesktop.Simulation;

namespace SiteDesktop.App.Scps;

public interface IScpModule
{
    string Status { get; }
    float Stress { get; }
    void SetActivity(bool active,bool disabled);
    void ApplyReactions(SystemOptions system,double delta);
    void Reset();
    void Shutdown();
}
