namespace SiteDesktop.Simulation;
// Immutable view of an ongoing effect, with no actor/state-machine reference.
public sealed record TreatmentPresentationState(int TargetId,double Age,bool Visible);
