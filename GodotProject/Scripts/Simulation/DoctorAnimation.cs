using Godot;
using SiteDesktop.Companions;
using SiteDesktop.Platforms;
using System;
using System.Linq;
using System.Text.Json;

namespace SiteDesktop.Simulation;

// Presentation state belongs to PlagueState. Rendering never advances its clock or game rules.
public sealed class DoctorAnimation
{
    public const string AssetPath="res://Assets/Sprites/characters/Scp049/";
    public SpriteAnimationSet Set { get; }=new(AssetPath+"Scp049.contract.json");
    public string Tag { get; private set; }="idle";
    public double Age { get; private set; }
    public bool Flipped { get; private set; }
    public int Frame=>Set.FrameAt(Tag,Age);
    public Vector2 ToolAnchor=>new(Flipped?128-_anchors[Frame].X*2:_anchors[Frame].X*2,_anchors[Frame].Y*2);
    private readonly Vector2[] _anchors;
    private int _transformations,_landings,_jumps;
    private PlatformMotion _motion;
    public DoctorAnimation()
    {
        using var doc=JsonDocument.Parse(Godot.FileAccess.GetFileAsString(AssetPath+"Scp049.contract.json"));
        _anchors=doc.RootElement.GetProperty("frames").EnumerateArray().Select(f=>{var p=f.GetProperty("treatmentAnchor");return new Vector2(p.GetProperty("x").GetSingle(),p.GetProperty("y").GetSingle());}).ToArray();
    }
    private void Select(string tag,double delta)
    {if(Tag!=tag){Tag=tag;Age=0;}else Age+=delta;}
    public void Reset(PlagueState state)
    {Tag="idle";Age=0;Flipped=false;_landings=state.Doctor.Body.Landings;_jumps=state.Doctor.Body.Jumps;_transformations=state.Transformations;_motion=state.Doctor.Body.Motion;}
    public void Update(PlagueState state,double delta)
    {
        if(!state.Visible||state.Stopped)return;
        var body=state.Doctor.Body;
        bool completed=state.Transformations>_transformations,landed=body.Landings>_landings,jumped=body.Jumps>_jumps;
        bool recovered=_motion==PlatformMotion.Balancing&&body.Motion==PlatformMotion.Grounded;
        _transformations=state.Transformations;_landings=body.Landings;_jumps=body.Jumps;_motion=body.Motion;
        if(state.TargetId is int id&&state.Objects.TryGetValue(id,out var target)&&Math.Abs(target.Foot.X-body.Foot.X)>1)Flipped=target.Foot.X<body.Foot.X;
        else if(Math.Abs(body.Velocity.X)>1)Flipped=body.Velocity.X<0;
        if(state.Phase==DoctorPhase.Dragging){Select("fall",delta);return;}
        if(body.SupportId==null)
        {
            if(jumped){Tag="takeoff";Age=0;}
            else if(Tag=="takeoff"&&Age+delta<Set.Duration("takeoff"))Age+=delta;
            else Select("fall",delta);
            return;
        }
        if(state.Phase==DoctorPhase.Treating)
        {
            double start=Set.Duration("treat_start");
            Tag=state.TreatmentAge<start?"treat_start":"treat_loop";
            Age=state.TreatmentAge<start?state.TreatmentAge:state.TreatmentAge-start;
            return;
        }
        if(completed){Tag="treat_end";Age=0;return;}
        if(landed){Tag="land";Age=0;return;}
        if(body.Motion==PlatformMotion.Balancing){Select("balance",delta);return;}
        if(recovered){Tag="balance_recover";Age=0;return;}
        if(Tag is "treat_end" or "land" or "balance_recover" && Age+delta<Set.Duration(Tag)){Age+=delta;return;}
        Select(Math.Abs(body.Velocity.X)>1?"move":"idle",delta);
    }
}
