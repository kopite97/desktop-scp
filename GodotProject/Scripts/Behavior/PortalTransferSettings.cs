namespace SiteDesktop.Behavior;
public sealed record PortalTransferSettings(double DepartureSeconds=.4,double HiddenSeconds=.2,double ArrivalSeconds=.4,double RemoteDelaySeconds=5)
{
    public double Duration=>DepartureSeconds+HiddenSeconds+ArrivalSeconds;
}
