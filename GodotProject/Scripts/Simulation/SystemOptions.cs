using System;

namespace SiteDesktop.Simulation;

public readonly record struct SystemSample(float? Cpu, float? Ram, float? Volume, bool Muted);

// Application-owned state. Unknown/stale signals never request an external effect.
public sealed class SystemOptions
{
    public bool Locked { get; private set; }
    public bool StrongEffects { get; private set; }
    public bool Stopped { get; private set; }
    public bool Hot { get; private set; }
    public bool Noisy { get; private set; }
    public SystemSample Sample { get; private set; }
    public long Generation { get; private set; }
    public double SampleAge { get; private set; } = double.PositiveInfinity;
    public bool CanAffectWindows => StrongEffects && !Locked && !Stopped;
    public void SetStrong(bool enabled) { if (!Stopped) StrongEffects = enabled; }
    public bool SetLocked(bool locked)
    {
        if (Stopped || Locked == locked) return false;
        Locked = locked; Generation++; SampleAge = double.PositiveInfinity;
        if (!locked) { Hot = false; Noisy = false; }
        return true;
    }
    private static float? Valid(float? v) => v is float f && float.IsFinite(f) && f >= 0 && f <= 100 ? f : null;
    public bool Accept(SystemSample sample, long generation)
    {
        if (Stopped || Locked || generation != Generation) return false;
        Sample = new(Valid(sample.Cpu), Valid(sample.Ram), Valid(sample.Volume), sample.Muted);
        SampleAge = 0;
        Hot = Hot ? Sample.Cpu >= 70 || Sample.Ram >= 75 : Sample.Cpu >= 80 || Sample.Ram >= 85;
        Noisy = !Sample.Muted && Sample.Volume >= (Noisy ? 65 : 70);
        return true;
    }
    public void Tick(double delta)
    {
        if (Stopped || Locked) return;
        SampleAge += Math.Clamp(delta, 0, .1);
        if (SampleAge > 5) { Hot = false; Noisy = false; }
    }
    public void Shutdown() { Stopped = true; Locked = true; StrongEffects = false; Generation++; Hot = false; Noisy = false; }
}
