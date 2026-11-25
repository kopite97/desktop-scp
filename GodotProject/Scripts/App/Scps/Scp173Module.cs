using System;
using SiteDesktop.Overlay;
using SiteDesktop.Simulation;
namespace SiteDesktop.App.Scps;
public sealed class Scp173Module(StatueController controller) : IScpModule
{
    public string Status=>$"{(controller.State.Observed?"Observed":"Unseen")} · Stress {Stress:0}";
    public float Stress=>controller.State.Stress.Value;
    public void SetActivity(bool active,ActivityReason reason){controller.Noisy=false;controller.SetVisible(active);}
    public void ApplyReactions(SystemOptions system,double delta){controller.Noisy=system.Noisy;if(system.Noisy)controller.State.Stress.Set(Stress+(float)Math.Clamp(delta,0,.1)*3);}
    public void Reset(){controller.State.Reset();controller.Publish();}
    public void Shutdown()=>controller.Shutdown();
}
