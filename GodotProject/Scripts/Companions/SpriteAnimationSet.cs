using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SiteDesktop.Companions;

public sealed record SpriteFrame(Rect2I Region,double Seconds,Vector2 ReactionAnchor,float BodyAlpha=1);
public sealed record SpriteClip(string Name,int From,int To,bool Loop);

public sealed class SpriteAnimationSet
{
    public SpriteFrame[] Frames { get; }
    public IReadOnlyDictionary<string,SpriteClip> Clips { get; }
    public Vector2I Cell { get; }
    public int Columns { get; }
    public int Rows { get; }
    public SpriteAnimationSet(string file)
    {
        using var doc=JsonDocument.Parse(Godot.FileAccess.GetFileAsString(file));
        var root=doc.RootElement;bool attachment=root.TryGetProperty("cell",out var canvas);var sheet=root.GetProperty("sheet");
        if(!attachment)canvas=root.GetProperty("canvas");
        Cell=new(canvas.GetProperty(attachment?"w":"width").GetInt32(),canvas.GetProperty(attachment?"h":"height").GetInt32());
        Columns=sheet.GetProperty("columns").GetInt32();Rows=sheet.GetProperty("rows").GetInt32();
        Frames=root.GetProperty("frames").EnumerateArray().Select(f=>{
            var r=f.GetProperty(attachment?"rect":"frame");var anchor=f.TryGetProperty("reactionAnchor",out var a)?new Vector2(a.GetProperty("x").GetSingle(),a.GetProperty("y").GetSingle()):Vector2.Zero;
            return new SpriteFrame(new Rect2I(r.GetProperty("x").GetInt32(),r.GetProperty("y").GetInt32(),r.GetProperty("w").GetInt32(),r.GetProperty("h").GetInt32()),f.GetProperty("durationMs").GetDouble()/1000,anchor,f.TryGetProperty("bodyAlpha",out var alpha)?alpha.GetSingle():1);
        }).ToArray();
        var clips=new Dictionary<string,SpriteClip>();
        foreach(var t in root.GetProperty("tags").EnumerateArray())
        {
            int indexOffset=attachment?1:0;
            var c=new SpriteClip(t.GetProperty("name").GetString()!,t.GetProperty("from").GetInt32()-indexOffset,t.GetProperty("to").GetInt32()-indexOffset,t.GetProperty("loop").GetBoolean());
            if(c.From<0||c.To<c.From||c.To>=Frames.Length)throw new InvalidDataException("Invalid animation range.");
            clips.Add(c.Name,c);
        }
        if(Frames.Any(f=>f.Seconds<=0)||Frames.Length!=Columns*Rows)throw new InvalidDataException("Invalid animation sheet/time contract.");
        Clips=clips;
    }
    public double Duration(string tag) { var c=Clips[tag];return Frames.Skip(c.From).Take(c.To-c.From+1).Sum(f=>f.Seconds); }
    public int FrameAt(string tag,double seconds,double? holdSeconds=null)
    {
        var c=Clips[tag];double duration=Duration(tag)+(holdSeconds.HasValue?holdSeconds.Value-Frames[c.From+2].Seconds:0);
        if(c.Loop)seconds%=duration;
        for(int i=c.From;i<=c.To;i++)
        {
            double ms=holdSeconds.HasValue && i==c.From+2?holdSeconds.Value:Frames[i].Seconds;
            if(seconds<ms)return i;seconds-=ms;
        }
        return c.To;
    }
}
