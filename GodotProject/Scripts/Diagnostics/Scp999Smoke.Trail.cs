using Godot;
using SiteDesktop.Companions;
using SiteDesktop.Overlay;
using System;
using System.IO;
using System.Threading.Tasks;
namespace SiteDesktop.Diagnostics;
public partial class Scp999Smoke
{
    // Exercise the product's exact effect renderer and frame clock, not a CPU rendering substitute.
    private async Task TrailRendering(Scp999Controller c)
    {
        const string path="res://Assets/Sprites/vfx/Scp999Trail/Scp999Trail.png";
        using var source=Image.LoadFromFile(ProjectSettings.GlobalizePath(path));
        using var texture=Scp999Window.LoadPng(path);
        int alphaErrors=0,lightingErrors=0,frameErrors=0,samples=0;
        using var contact=Image.CreateEmpty(64*9,32*2,false,Image.Format.Rgba8);
        foreach(var clip in c.State.Trails.Clips.Values)
        {
            var effect=new CompanionEffect{Kind=CompanionEffectKind.Trail,Tag=clip.Name,Origin=c.State.Foot-new Vector2(0,180)};
            var window=new ScpEffectWindow(effect,texture,c.State.Trails);AddChild(window);
            var ambient=new CanvasModulate();window.AddChild(ambient);
            using var white=Image.CreateEmpty(128,128,false,Image.Format.Rgba8);white.Fill(Colors.White);
            using var lightTexture=ImageTexture.CreateFromImage(white);
            var light=new PointLight2D{Texture=lightTexture,Position=new(32,16),Color=new Color(1,.5f,.1f),Energy=2};window.AddChild(light);
            try
            {
                double age=0;
                for(int f=clip.From;f<=clip.To;f++)
                {
                    effect.Age=age+.001;ambient.Color=Colors.White;light.Enabled=false;window.Publish(c.State);await Frames(3);
                    using var normal=window.GetTexture().GetImage();
                    if(window.DisplayedFrame!=f)frameErrors++;
                    ambient.Color=new Color(.15f,.25f,.4f);light.Enabled=true;await Frames(3);
                    using var changed=window.GetTexture().GetImage();
                    var region=c.State.Trails.Frames[f].Region;
                    for(int y=0;y<32;y++)for(int x=0;x<64;x++)
                    {
                        var a=normal.GetPixel(x,y);var b=changed.GetPixel(x,y);
                        if(Math.Abs(a.A-source.GetPixel(region.Position.X+x/2,region.Position.Y+y/2).A)>.004)alphaErrors++;
                        if(Math.Abs(a.R-b.R)+Math.Abs(a.G-b.G)+Math.Abs(a.B-b.B)+Math.Abs(a.A-b.A)>.012)lightingErrors++;
                    }
                    contact.BlitRect(normal,new(0,0,64,32),new((f%9)*64,(f/9)*32));samples++;
                    age+=f==clip.From+2?c.State.Settings.TrailHoldSeconds:c.State.Trails.Frames[f].Seconds;
                }
            }
            finally{window.Hide();window.QueueFree();await Frames(2);}
        }
        contact.SavePng(Path.Combine(_output,"trail-engine-frames.png"));
        File.WriteAllText(Path.Combine(_output,"trail-render.json"),System.Text.Json.JsonSerializer.Serialize(new{samples,alphaErrors,lightingErrors,frameErrors,scale=2,revision="r001-approved"}));
        Require(samples==18&&frameErrors==0,"approved trail plays all 18 product frames with extended hold");
        Require(alphaErrors==0,"approved trail preserves source alpha with nearest 2x sampling");
        Require(lightingErrors==0,"approved trail is invariant under ambient color and point light changes");
    }
}
