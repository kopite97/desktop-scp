using SiteDesktop.Overlay;
using SiteDesktop.Platforms;
using SiteDesktop.Simulation;
using System.Collections.Generic;

namespace SiteDesktop.Diagnostics;
// Scenario orchestration only: every operation invokes a product owner once.
// Product actors no longer advance or hide the shared world implicitly.
public static class PlagueScenario
{
    public static void ReplaceScreens(this PlagueState doctor,IReadOnlyList<SiteDesktop.Desktop.DesktopScreen> screens){doctor.World.ReplaceScreens(screens);doctor.ReplaceDoctorScreens(screens);}
    public static void Tick(this PlagueState doctor,double dt){doctor.World.Tick(dt);doctor.TickDoctor(dt);}
    public static void Observe(this PlagueState doctor,IReadOnlyList<WindowObservation> windows){doctor.World.Observe(windows);doctor.ObserveDoctor(windows);}
    public static void SetVisible(this PlagueState doctor,bool visible){doctor.World.SetActivity(visible,visible);doctor.SetDoctorActive(visible,false);}
    public static void Advance(this PlagueController controller,double dt){controller.Objects.Advance(dt);controller.State.TickDoctor(dt);controller.Publish();}
    public static void Publish(this PlagueController controller){controller.PublishBody();controller.Objects.Publish();}
    public static void Refresh(this PlagueController controller){controller.Objects.Refresh();controller.RefreshBody();}
    public static void SetVisible(this PlagueController controller,bool visible){controller.Objects.SetActivity(visible,visible);controller.SetActive(visible,false);}
}
