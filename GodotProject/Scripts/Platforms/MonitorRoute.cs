using SiteDesktop.Desktop;
namespace SiteDesktop.Platforms;
public sealed record MonitorRoute(DesktopScreen Source,DesktopScreen Destination,MovementGoal Departure,MovementGoal Arrival);
