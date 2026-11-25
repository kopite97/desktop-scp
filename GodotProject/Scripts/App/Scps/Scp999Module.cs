using SiteDesktop.Overlay;
using SiteDesktop.Simulation;
namespace SiteDesktop.App.Scps;
public sealed class Scp999Module(Scp999Controller controller) : IScpModule
{
    public string Status=>$"{controller.State.Tag} · {controller.InputStatus}";
    public float Stress=>0;
    public void SetActivity(bool active,ActivityReason reason)=>controller.SetVisible(active,reason==ActivityReason.Disabled);
    public void ApplyReactions(SystemOptions system,double delta){controller.State.Overheated=system.Hot;controller.State.Physics.SpeedScale=system.Hot?.6f:1;}
    public void Reset()=>controller.ResetPositions();
    public void Shutdown()=>controller.Shutdown();
}
