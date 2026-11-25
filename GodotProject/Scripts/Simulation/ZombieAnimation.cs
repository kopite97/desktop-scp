using SiteDesktop.Companions;
using SiteDesktop.Platforms;
using System;

namespace SiteDesktop.Simulation;

// Authored presentation only. The shared world ZombieClock pauses with its owner; Publish never advances it.
public sealed class ZombieAnimation
{
    public const string AssetPath="res://Assets/Sprites/characters/Scp0492/";
    public static SpriteAnimationSet Set { get; }=new(AssetPath+"Scp0492.contract.json");
    public string Tag { get; private set; }="spawn";
    public double Age { get; private set; }
    public bool Flipped { get; private set; }
    public int Frame=>FrameAt(Tag,Age);
    // r001 move is authored with its planted foot travelling forward locally.
    // Reverse only that cycle: stance goes backward, swing returns forward.
    // Keep each source frame's duration, both feet in phase and facing unchanged.
    public static int FrameAt(string tag,double age)
    {
        if(tag!="move")return Set.FrameAt(tag,age);
        var clip=Set.Clips[tag];double remaining=age%Set.Duration(tag);
        for(int frame=clip.To;frame>=clip.From;frame--)
        {
            if(remaining<Set.Frames[frame].Seconds)return frame;
            remaining-=Set.Frames[frame].Seconds;
        }
        return clip.From;
    }
    private double _clock;
    private int _landings;
    public ZombieAnimation(DummyObject item,double clock){_clock=clock;_landings=item.Physics.Body.Landings;}
    public void Update(DummyObject item,double clock)
    {
        double delta=Math.Max(0,clock-_clock);_clock=clock;var body=item.Physics.Body;
        bool landed=body.Landings>_landings;_landings=body.Landings;
        if(Math.Abs(body.Velocity.X)>1)Flipped=body.Velocity.X<0;
        string next;
        if(body.Motion==PlatformMotion.Dragging||body.SupportId==null)next="fall";
        else if(landed){Tag="land";Age=0;return;}
        else if(Tag is "spawn" or "land" && Age+delta<Set.Duration(Tag)){Age+=delta;return;}
        else next=Math.Abs(body.Velocity.X)>1?"move":"idle";
        if(next!=Tag){Tag=next;Age=0;}else Age+=delta;
    }
}
