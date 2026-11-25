using SiteDesktop.Overlay;
using SiteDesktop.Simulation;
namespace SiteDesktop.App.Scps;
public sealed class Scp049Module(PlagueController controller) : IScpModule
{
    public string Status=>$"{controller.State.Phase} · Stress {Stress:0}";
    public float Stress=>controller.State.DoctorStress.Value;
    public void SetActivity(bool active,ActivityReason reason)=>controller.SetActive(active,reason==ActivityReason.Disabled);
    public void ApplyReactions(SystemOptions system,double delta){controller.State.Doctor.SpeedScale=system.Hot?1.5f:1;controller.State.TreatmentRate=system.Hot?1.5f:1;}
    public void Reset()=>controller.ResetDoctor();
    public void Shutdown()=>controller.Shutdown();
}
