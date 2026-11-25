using Godot;
using SiteDesktop.Companions;
using SiteDesktop.Desktop;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System;
using System.IO;
using System.Text.Json;

namespace SiteDesktop.Diagnostics;

public partial class Scp999ColorProbe : Node
{
    public override async void _Ready()
    {
        try
        {
            string output="";var args=OS.GetCmdlineUserArgs();for(int i=0;i+1<args.Length;i++)if(args[i]=="--output")output=args[++i];
            if(output==""||File.Exists(Path.Combine(output,"results.json")))throw new InvalidOperationException("New output required.");Directory.CreateDirectory(output);
            var screen=DesktopPlacement.Select();var root=GetWindow();root.Size=new(620,280);root.Position=WindowsDesktop.ToGodotPosition(screen.Usable.Position+new Vector2I(120,120));root.Title="SCP-999 — brighter orange";root.AlwaysOnTop=true;
            AddChild(new ColorRect{Color=new Color("283747"),Size=new(620,280)});
            AddChild(new Label{Text="BEFORE",Position=new(98,26)});AddChild(new Label{Text="BRIGHTER ORANGE",Position=new(360,26)});
            var body=new SpriteAnimationSet(Scp999Window.AssetPath+"Scp999.contract.json");
            var reaction=new SpriteAnimationSet("res://Assets/Sprites/vfx/Scp999Reaction/Scp999Reaction.contract.json");
            var trail=new SpriteAnimationSet("res://Assets/Sprites/vfx/Scp999Trail/Scp999Trail.contract.json");
            var old=new Scp999Window();var bright=new Scp999Window();AddChild(old);AddChild(bright);
            old.SetLighting(.42f,.72f,new Color(1,.95f,.88f));
            foreach(var (window,x) in new[]{(old,80),(bright,370)})
            {
                var state=new Scp999State(new[]{screen},body,reaction,trail,new StressState());
                state.BeginDrag(state.Position);state.Tick(0,screen.Usable.Position+new Vector2(120+x,200),false,true);window.Publish(state);
            }
            for(int i=0;i<10;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            using var before=old.GetTexture().GetImage();using var after=bright.GetTexture().GetImage();
            before.SavePng(Path.Combine(output,"before.png"));after.SavePng(Path.Combine(output,"after.png"));
            var bounds=WindowsDesktop.GetBounds(root);
            using(var shot=DisplayServer.ScreenGetImageRect(new Rect2I(root.Position,root.Size)))
                shot.SavePng(Path.Combine(output,"comparison.png"));
            double a=0,b=0;int count=0,alphaErrors=0,outlineErrors=0;
            using var outline=Image.LoadFromFile(ProjectSettings.GlobalizePath(Scp999Window.AssetPath+"Scp999_outline.png"));
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                var p=before.GetPixel(x,y);var q=after.GetPixel(x,y);
                if(Math.Abs(p.A-q.A)>.004)alphaErrors++;
                if(outline.GetPixel(x/2,y/2).A>0){if(Math.Abs(p.R-q.R)+Math.Abs(p.G-q.G)+Math.Abs(p.B-q.B)>.012)outlineErrors++;}
                else if(p.A>.2){a+=.2126*p.R+.7152*p.G+.0722*p.B;b+=.2126*q.R+.7152*q.G+.0722*q.B;count++;}
            }
            bool pass=b>a*1.3&&alphaErrors==0&&outlineErrors==0;
            File.WriteAllText(Path.Combine(output,"results.json"),JsonSerializer.Serialize(new{passed=pass,bodyPixels=count,beforeMean=a/count,afterMean=b/count,ratio=b/a,alphaErrors,outlineErrors,sourceRevision="r005 unchanged",ambient=.9,keyEnergy=.5},new JsonSerializerOptions{WriteIndented=true}));
            old.Hide();bright.Hide();old.QueueFree();bright.QueueFree();
            if(!pass)throw new InvalidOperationException("Brightness/alpha/outline comparison failed.");
            GD.Print("SCP999_COLOR_PASS");GetTree().Quit();
        }
        catch(Exception ex){GD.PushError("SCP999_COLOR_FAILED "+ex);GetTree().Quit(1);}
    }
}
