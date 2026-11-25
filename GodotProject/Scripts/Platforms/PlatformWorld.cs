using Godot;
using SiteDesktop.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteDesktop.Platforms;

// Same one-way collision solver for native observations and replay inputs.
public sealed class PlatformWorld
{
    public PlatformSettings Settings { get; }
    public PlatformBody Body { get; } = new();
    public float SpeedScale { get; set; } = 1;
    public IReadOnlyList<DesktopScreen> Screens { get; private set; }
    public IReadOnlyList<PlatformSurface> Surfaces { get; private set; } = Array.Empty<PlatformSurface>();
    public bool Visible { get; private set; } = true;
    public bool Stopped { get; private set; }
    public long Revision { get; private set; }
    public float WalkDistance { get; private set; }
    public Vector2 CarryDelta { get; private set; }

    public PlatformWorld(IReadOnlyList<DesktopScreen> screens, PlatformSettings? settings = null)
    {
        if (screens.Count == 0) throw new ArgumentException("A work area is required.");
        Screens = screens; Settings = settings ?? new();
        Observe(Array.Empty<WindowObservation>()); Reset();
    }

    public void Observe(IReadOnlyList<WindowObservation> windows)
    {
        if (!Visible || Stopped) return;
        var surfaces = new List<PlatformSurface>();
        var occluders = new List<Rect2I>();
        foreach (var w in windows)
        {
            if (!w.Visible || w.Minimized || w.Cloaked || w.Bounds.Size.X <= 0 || w.Bounds.Size.Y <= 0) continue;
            foreach (var screen in Screens)
            {
                var area = screen.Usable;
                float y = w.Bounds.Position.Y;
                if (!w.CanSupport || y < area.Position.Y + Settings.Height || y >= area.End.Y) continue;
                var pieces = new List<(float Left, float Right)> { (Math.Max(w.Bounds.Position.X, area.Position.X), Math.Min(w.Bounds.End.X, area.End.X)) };
                foreach (var cover in occluders)
                {
                    // Require clearance above the whole body, not only a visible 1px edge.
                    if (cover.Position.Y >= y || cover.End.Y <= y - Settings.Height) continue;
                    var next = new List<(float, float)>();
                    foreach (var (left, right) in pieces)
                    {
                        if (cover.End.X <= left || cover.Position.X >= right) next.Add((left, right));
                        else
                        {
                            if (cover.Position.X > left) next.Add((left, cover.Position.X));
                            if (cover.End.X < right) next.Add((cover.End.X, right));
                        }
                    }
                    pieces = next;
                }
                foreach (var (left, right) in pieces)
                    if (right - left >= Settings.HalfWidth * 2)
                        surfaces.Add(new(w.Id, left, right, y, w.Bounds.Position));
            }
            if (w.Blocks) occluders.Add(w.Bounds);
        }
        foreach (var screen in Screens)
            surfaces.Add(new($"floor:{screen.Index}", screen.Usable.Position.X, screen.Usable.End.X,
                screen.Usable.End.Y - 8, screen.Usable.Position, true));
        if(!Surfaces.SequenceEqual(surfaces))Revision++;
        Surfaces = surfaces;CarryDelta=Vector2.Zero;
        ReconcileSupport();
    }

    private void ReconcileSupport()
    {
        if (Body.SupportId == null) return;
        var candidates = Surfaces.Where(s => s.Id == Body.SupportId);
        var next = candidates.FirstOrDefault(s => s.Contains(Body.Foot.X + s.WindowOrigin.X - Body.SupportOrigin.X, Settings.HalfWidth));
        if (next == null) { Fall(); return; }
        Vector2 delta = next.WindowOrigin - Body.SupportOrigin;
        if (delta.Length() > Settings.MaxCarryDelta) { Fall(); return; }
        CarryDelta+=delta;Body.Foot += delta; Body.SupportOrigin = next.WindowOrigin;
        if (delta.LengthSquared() > 0)
        {
            Body.Slip = Math.Clamp(Body.Slip - delta.X * 3, -320, 320);
            Body.Motion = PlatformMotion.Balancing; Body.PoseTime = .25;
        }
    }

    private void Fall()
    {
        float vx = Body.SupportId != null ? Body.WalkDirection * Settings.Speed * SpeedScale + Body.Slip : Body.Velocity.X;
        Body.SupportId = null;
        Body.Motion = PlatformMotion.Falling;
        Body.Velocity = new(vx, Math.Max(0, Body.Velocity.Y));
        Body.Slip = 0;
    }

    public void Reset()
    {
        if (Stopped) return;
        var area = Screens[0].Usable;
        Place(new Vector2(area.Position.X + 280, area.End.Y - 180));
        Body.WalkDirection = 0;
    }

    public void Place(Vector2 foot)
    {
        if (Stopped) return;
        Body.Foot = foot; Body.Velocity = Vector2.Zero; Body.SupportId = null;
        Body.Motion = PlatformMotion.Falling; Body.Slip = 0; Body.PoseTime = 0;
    }

    public void Walk(int direction) { if (!Stopped) Body.WalkDirection = Math.Sign(direction); }

    public bool JumpToAdjacent(int direction = 0)
    {
        if (Stopped || !Visible || Body.SupportId == null || Body.JumpCooldown > 0) return false;
        var targets = Surfaces.Where(s => s.Id != Body.SupportId && !s.Floor && s.Right - s.Left >= Settings.HalfWidth * 2 + 4 &&
            Body.Foot.Y - s.Y <= Settings.MaxJumpRise && s.Y - Body.Foot.Y <= 180)
            .Select(s => (Surface: s, X: Math.Clamp(Body.Foot.X, s.Left + Settings.HalfWidth + 2, s.Right - Settings.HalfWidth - 2)))
            .Where(t => Math.Abs(t.X - Body.Foot.X) <= Settings.MaxJumpGap && (direction == 0 || Math.Sign(t.X - Body.Foot.X) == direction))
            .OrderBy(t => Math.Abs(t.X - Body.Foot.X) + Math.Abs(t.Surface.Y - Body.Foot.Y)).ToArray();
        foreach (var target in targets)if(TryJumpTo(target.Surface))return true;
        return false;
    }
    public bool JumpSolution(Vector2 from,PlatformSurface target,out float vx,out float x)
    {
        vx=0;x=from.X;
        if(target.Floor||target.Right-target.Left<Settings.HalfWidth*2+4||from.Y-target.Y>Settings.MaxJumpRise||target.Y-from.Y>180)return false;
        x=Math.Clamp(from.X,target.Left+Settings.HalfWidth+2,target.Right-Settings.HalfWidth-2);
        if(Math.Abs(x-from.X)>Settings.MaxJumpGap)return false;
        float disc=Settings.JumpSpeed*Settings.JumpSpeed+2*Settings.Gravity*(target.Y-from.Y);
        if(disc<=0)return false;
        float time=(Settings.JumpSpeed+MathF.Sqrt(disc))/Settings.Gravity;
        vx=(x-from.X)/time;if(Math.Abs(vx)>400)return false;
        float horizontal=vx;
        var first=Surfaces.Select(surface=>
        {
            float d=Settings.JumpSpeed*Settings.JumpSpeed+2*Settings.Gravity*(surface.Y-from.Y);
            float t=d<0?float.PositiveInfinity:(Settings.JumpSpeed+MathF.Sqrt(d))/Settings.Gravity;
            return (Surface:surface,Time:t);
        }).Where(t=>float.IsFinite(t.Time)&&t.Surface.Contains(from.X+horizontal*t.Time,Settings.HalfWidth)).OrderBy(t=>t.Time).FirstOrDefault();
        return first.Surface==target;
    }
    public bool TryJumpTo(PlatformSurface target)
    {
        if(Stopped||!Visible||Body.SupportId==null||Body.JumpCooldown>0||!JumpSolution(Body.Foot,target,out float vx,out _))return false;
        Body.Velocity=new(vx,-Settings.JumpSpeed);Body.SupportId=null;Body.Motion=PlatformMotion.Takeoff;
        Body.JumpCooldown=.6;Body.Slip=0;Body.Jumps++;return true;
    }

    public void BeginDrag(Vector2 pointer)
    {
        if (!Visible || Stopped) return;
        Body.DragOffset = pointer - Body.Foot; Body.SupportId = null; Body.Motion = PlatformMotion.Dragging;
        Body.Velocity = Vector2.Zero; Body.Slip = 0;
    }
    public void Drag(Vector2 pointer)
    {
        if (Visible && !Stopped && Body.Motion == PlatformMotion.Dragging) Body.Foot = ConstrainFoot(pointer - Body.DragOffset);
    }
    public void EndDrag() { if (Body.Motion == PlatformMotion.Dragging) Fall(); }
    private Vector2 ConstrainFoot(Vector2 foot)
    {
        var top = foot - new Vector2(Settings.HalfWidth, Settings.Height);
        var constrained = DesktopGeometry.Constrain((Vector2I)top, new Vector2I((int)Settings.HalfWidth * 2, (int)Settings.Height), Screens) + new Vector2(Settings.HalfWidth, Settings.Height);
        var area = Screens.OrderBy(s => ((Vector2)s.Usable.GetCenter()).DistanceSquaredTo(constrained)).First().Usable;
        constrained.Y = Math.Min(constrained.Y, area.End.Y - 8);
        return constrained;
    }

    public void Tick(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
        WalkDistance=0;
        if (!Visible || Stopped || Body.Motion == PlatformMotion.Dragging) return;
        double remaining = Math.Min(seconds, .1);
        while (remaining > .0000001)
        {
            float dt = (float)Math.Min(remaining, 1d / 120); remaining -= dt;
            Body.JumpCooldown = Math.Max(0, Body.JumpCooldown - dt); Body.PoseTime = Math.Max(0, Body.PoseTime - dt);
            if (Body.SupportId != null)
            {
                float vx = Body.WalkDirection * Settings.Speed * SpeedScale + Body.Slip;
                Body.Slip = Mathf.MoveToward(Body.Slip, 0, 600 * dt);
                var support = Surfaces.FirstOrDefault(s => s.Id == Body.SupportId && s.Contains(Body.Foot.X, Settings.HalfWidth));
                if (support == null) { Fall(); continue; }
                if (!support.Floor && Body.AutoJump && Body.WalkDirection != 0 &&
                    !support.Contains(Body.Foot.X + Body.WalkDirection * 36, Settings.HalfWidth) && JumpToAdjacent(Body.WalkDirection)) continue;
                float x = Body.Foot.X + vx * dt;
                if (support.Floor) x = Math.Clamp(x, support.Left + Settings.HalfWidth, support.Right - Settings.HalfWidth);
                float actual=x-Body.Foot.X;
                if(Body.WalkDirection!=0&&Body.Motion==PlatformMotion.Grounded)WalkDistance+=Math.Abs(actual);
                Body.Foot = new(x, support.Y);
                Body.Velocity = new(actual/dt, 0);
                if (!support.Contains(x, Settings.HalfWidth)) Fall();
                else if (Body.PoseTime <= 0) Body.Motion = PlatformMotion.Grounded;
            }
            else
            {
                var old = Body.Foot;
                var next = old + Body.Velocity * dt + new Vector2(0, .5f * Settings.Gravity * dt * dt);
                Body.Velocity += new Vector2(0, Settings.Gravity * dt);
                var hit = Body.Velocity.Y >= 0 ? Surfaces.Where(s => old.Y <= s.Y + .01f && next.Y >= s.Y)
                    .Where(s => s.Contains(Mathf.Lerp(old.X, next.X, Math.Clamp((s.Y - old.Y) / Math.Max(.00001f, next.Y - old.Y), 0, 1)), Settings.HalfWidth))
                    .OrderBy(s => s.Y).FirstOrDefault() : null;
                if (hit != null)
                {
                    Body.Foot = new(Math.Clamp(next.X, hit.Left + Settings.HalfWidth, hit.Right - Settings.HalfWidth), hit.Y);
                    Body.Velocity = Vector2.Zero; Body.SupportId = hit.Id; Body.SupportOrigin = hit.WindowOrigin;
                    Body.Motion = PlatformMotion.Landing; Body.PoseTime = .2; Body.Landings++;
                }
                else { Body.Foot = next; Body.Motion = Body.Velocity.Y < 0 ? PlatformMotion.Takeoff : PlatformMotion.Falling; }
                // Recover if a flight leaves every usable desktop region (gaps/monitor removal).
                if (!Screens.Any(s => s.Usable.Grow(120).HasPoint((Vector2I)Body.Foot)))
                { Body.Foot = ConstrainFoot(Body.Foot); Body.Velocity = Vector2.Zero; }
            }
        }
    }

    public void ReplaceScreens(IReadOnlyList<DesktopScreen> screens)
    {
        if (Stopped || screens.Count == 0) return;
        Screens = screens; Place(ConstrainFoot(Body.Foot)); Observe(Array.Empty<WindowObservation>());
    }
    public void SetVisible(bool visible)
    {
        if (Stopped) return;
        if (!visible) EndDrag();
        Visible = visible;
    }
    public void Stop() { if (Stopped) return; Visible = false; Stopped = true; Body.WalkDirection = 0; Body.SupportId = null; }
}
