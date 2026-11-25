using Godot;
using SiteDesktop.Platforms;
using System.Collections.Generic;

namespace SiteDesktop.Desktop;
public sealed record DesktopEnvironmentSnapshot(long Version,double ObservedAt,IReadOnlyList<DesktopScreen> Screens,
    IReadOnlyList<DesktopScreen> AllScreens,IReadOnlyList<WindowObservation> Windows,Vector2? Pointer,bool Left,bool Right);

public interface IDesktopEnvironmentSource
{
    DesktopEnvironmentSnapshot Current { get; }
    DesktopEnvironmentSnapshot Capture(double delta,bool force=false);
}
