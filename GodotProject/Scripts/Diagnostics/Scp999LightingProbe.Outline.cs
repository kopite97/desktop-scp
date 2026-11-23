using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;

// ART-002 follow-up checks; old revisions retain their single-sprite path.
public partial class Scp999LightingProbe
{
    private Sprite2D? _outline;
    private Node2D? _visualRoot;
    private Image? _outlineImage;
    private Texture2D? _splitDiffuse;
    private int _renderScale=2;
    private Sprite2D[]? _reactionPreview;
    private double _reactionMs;
    private List<object>? _reactionTrace;
    private readonly Stopwatch _reactionClock=new();
    private int _reactionPhase;
    private readonly int[] _reactionDurations={70,80,100,160,160,120,100,60};
    private void UpdateReactionPreview(double delta)
    {
        if(_reactionPreview==null)return;
        int previousPhase=_reactionPhase;
        _reactionMs=_reactionClock.Elapsed.TotalMilliseconds%1200;
        double t=_reactionMs;_reactionPhase=0;
        while(_reactionPhase<8&&t>=_reactionDurations[_reactionPhase])t-=_reactionDurations[_reactionPhase++];
        for(int i=0;i<2;i++){_reactionPreview[i].Visible=_reactionPhase<8;if(_reactionPhase<8)_reactionPreview[i].Frame=i*8+_reactionPhase;}
        if(previousPhase!=_reactionPhase)_reactionTrace?.Add(new {phase=_reactionPhase,ms=_reactionClock.Elapsed.TotalMilliseconds,visible=_reactionPreview[0].Visible});
    }
    private void BuildOutline(string path)
    {
        _splitDiffuse=ImageTexture.CreateFromImage(Image.LoadFromFile(ProjectSettings.GlobalizePath(path+"Scp999_color.png")));
        _outlineImage=Image.LoadFromFile(ProjectSettings.GlobalizePath(path+"Scp999_outline.png"));
        _visualRoot=new Node2D();_viewport.AddChild(_visualRoot);_sprite.Reparent(_visualRoot);
        _outline=new Sprite2D {Texture=ImageTexture.CreateFromImage(_outlineImage),Hframes=_count,Position=_sprite.Position,Scale=_sprite.Scale,TextureFilter=CanvasItem.TextureFilterEnum.Nearest,
            Material=new CanvasItemMaterial {LightMode=CanvasItemMaterial.LightModeEnum.Unshaded,BlendMode=CanvasItemMaterial.BlendModeEnum.Mix}};
        _visualRoot.AddChild(_outline);
    }
    private void ApplyOutlineState(int frame,bool flip,string mode)
    {
        if(_outline==null)return;
        _material.DiffuseTexture=mode=="legacy"?_diffuse:_splitDiffuse;
        _outline.Frame=frame;_outline.FlipH=flip;_outline.Visible=mode=="lit"||mode=="normal";
        _sprite.Scale=_outline.Scale=Vector2.One*_renderScale;
        _light.Height=50*_renderScale;
        _light.Position=new Vector2(64,64)+_directions[_direction]*(_renderScale/2f);
    }
    private static double RgbError(Color a,Color b)=>Math.Max(Math.Abs(a.R-b.R),Math.Max(Math.Abs(a.G-b.G),Math.Abs(a.B-b.B)));
    private async Task VerifyOutlineAndReaction()
    {
        int outlineErrors=0,alphaErrors=0,syncErrors=0;double bodyResponse=0,maxOutlineError=0;
        int[] selected=_count==4?new[]{0,1,2,3}:new[]{0,6,8,11};
        var comparison=Image.CreateEmpty(512,512,false,Image.Format.Rgba8);comparison.Fill(new Color(.17f,.19f,.23f));
        foreach(bool night in new[]{false,true})for(int f=0;f<_count;f++)
        {
            SetState(f,0,false,night,"legacy");using var old=await Capture();int col=Array.IndexOf(selected,f);
            if(col>=0) {
                comparison.BlendRect(old,new Rect2I(0,0,128,128),new Vector2I(col*128,night?256:0));
                comparison.BlendRect(_renders[Key(f,0,false,night,"lit")],new Rect2I(0,0,128,128),new Vector2I(col*128,night?384:128));
            }
        }
        comparison.SavePng(Path.Combine(_output,"outline-before-after.png"));
        var one=Image.CreateEmpty(512,256,false,Image.Format.Rgba8);one.Fill(new Color(.17f,.19f,.23f));
        foreach(int scale in new[]{2,1})foreach(bool night in new[]{false,true})foreach(bool flip in new[]{false,true})
        for(int direction=0;direction<3;direction++)for(int f=0;f<_count;f++)
        {
            _renderScale=scale;SetState(f,direction,flip,night,"lit");
            Image im=scale==2?_renders[Key(f,direction,flip,night,"lit")]:await Capture();
            if(_sprite.Frame!=_outline!.Frame||_sprite.FlipH!=_outline.FlipH||_sprite.Scale!=_outline.Scale)syncErrors++;
            int origin=64-32*scale;
            for(int y=0;y<64;y++)for(int x=0;x<64;x++) {
                int sx=flip?63-x:x;var edge=_outlineImage!.GetPixel(f*64+sx,y);var expected=_color.GetPixel(f*64+sx,y);
                for(int iy=0;iy<scale;iy++)for(int ix=0;ix<scale;ix++) {
                    var actual=im.GetPixel(origin+x*scale+ix,origin+y*scale+iy);
                    if(Math.Abs(expected.A-actual.A)>1.1/255)alphaErrors++;
                    if(edge.A>0) {double e=RgbError(edge,actual);maxOutlineError=Math.Max(maxOutlineError,e);if(e>1.1/255)outlineErrors++;}
                }
            }
            int col=Array.IndexOf(selected,f);
            if(scale==1&&col>=0&&direction==0&&!flip)one.BlendRect(im,new Rect2I(0,0,128,128),new Vector2I(col*128,night?128:0));
            if(scale==1)im.Dispose();
        }
        one.SavePng(Path.Combine(_output,"outline-1x.png"));
        _renderScale=2;SetState(0,0,false,false,"lit");
        var left=_renders[Key(0,0,false,false,"lit")];var right=_renders[Key(0,1,false,false,"lit")];
        for(int y=0;y<64;y++)for(int x=0;x<64;x++)if(_outlineImage!.GetPixel(x,y).A==0&&_color.GetPixel(x,y).A>0)
            bodyResponse+=RgbError(left.GetPixel(x*2,y*2),right.GetPixel(x*2,y*2));
        _visualRoot!.Modulate=new Color(.4f,.5f,.6f,1);using var badParent=await Capture();
        _visualRoot.Modulate=Colors.White;_sprite.SelfModulate=new Color(.4f,.5f,.6f,1);using var goodSelf=await Capture();
        double inheritedChange=0,selfOutlineError=0;
        for(int y=0;y<64;y++)for(int x=0;x<64;x++)if(_outlineImage!.GetPixel(x,y).A>0) {
            inheritedChange=Math.Max(inheritedChange,RgbError(left.GetPixel(x*2,y*2),badParent.GetPixel(x*2,y*2)));
            selfOutlineError=Math.Max(selfOutlineError,RgbError(left.GetPixel(x*2,y*2),goodSelf.GetPixel(x*2,y*2)));
        }
        _sprite.SelfModulate=Colors.White;
        bool reaction=await VerifyReaction();
        bool passed=outlineErrors==0&&alphaErrors==0&&syncErrors==0&&bodyResponse>0&&inheritedChange>.01&&selfOutlineError<1.1/255&&reaction;
        WriteJson("outline-results.json",new {passed,frames=_count,scales=new[]{1,2},conditions=_count*24,
            outlineErrors,alphaErrors,syncErrors,maxOutlineError,bodyResponse,inheritedParentNegativeControl=inheritedChange,
            bodySelfModulateOutlineError=selfOutlineError,reactionPassed=reaction,
            scope="Actual Compatibility rendering; diagnostic path, not product behavior or final user approval"});
        if(!passed)throw new InvalidDataException("Outline/reaction gate failed; inspect JSON.");
    }
    private async Task<bool> VerifyReaction()
    {
        var path="res://Assets/Diagnostics/Scp999Lighting/reaction-r001/";
        using var source=Image.LoadFromFile(ProjectSettings.GlobalizePath(path+"Scp999Reaction.png"));
        var view=new SubViewport {Size=new Vector2I(64,64),TransparentBg=true,Disable3D=true,RenderTargetUpdateMode=SubViewport.UpdateMode.Always};AddChild(view);
        var root=new Node2D();view.AddChild(root);
        var fx=new Sprite2D {Texture=ImageTexture.CreateFromImage(source),Hframes=8,Vframes=2,Position=new Vector2(32,32),Scale=new Vector2(2,2),
            TextureFilter=CanvasItem.TextureFilterEnum.Nearest,Material=new CanvasItemMaterial {LightMode=CanvasItemMaterial.LightModeEnum.Unshaded,BlendMode=CanvasItemMaterial.BlendModeEnum.Mix}};root.AddChild(fx);
        var ambient=new CanvasModulate();view.AddChild(ambient);
        using var white=Image.CreateEmpty(256,256,false,Image.Format.Rgba8);white.Fill(Colors.White);
        var light=new PointLight2D {Texture=ImageTexture.CreateFromImage(white),Position=new Vector2(0,0),Height=50};view.AddChild(light);
        int colorErrors=0,alphaErrors=0,endPixels=0;double maxColorError=0;
        var sheet=Image.CreateEmpty(512,256,false,Image.Format.Rgba8);sheet.Fill(new Color(.17f,.19f,.23f));
        Directory.CreateDirectory(Path.Combine(_output,"reaction"));
        for(int context=0;context<4;context++)for(int f=0;f<16;f++)
        {
            fx.Frame=f;ambient.Color=context==0?Colors.White:context==1?new Color(.12f,.15f,.25f):new Color(.45f,.32f,.20f);
            light.Enabled=context!=0;light.Energy=context==2?2:.3f;light.Color=context==2?new Color(1,.2f,.1f):new Color(.3f,.6f,1);
            light.Position=context==3?new Vector2(100,-50):new Vector2(-50,-50);
            for(int i=0;i<3;i++)await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using var im=view.GetTexture().GetImage();im.Convert(Image.Format.Rgba8);
            for(int y=0;y<64;y++)for(int x=0;x<64;x++) {
                var expected=source.GetPixel((f%8)*32+x/2,(f/8)*32+y/2);var c=im.GetPixel(x,y);
                if(Math.Abs(expected.A-c.A)>1.1/255)alphaErrors++;
                if(c.A>0) {
                    c=new Color(c.R/c.A,c.G/c.A,c.B/c.A,c.A);im.SetPixel(x,y,c);
                    double e=RgbError(expected,c);maxColorError=Math.Max(maxColorError,e);
                    if(e>1.1/(255*c.A))colorErrors++;
                }
                if((f==7||f==15)&&c.A>0)endPixels++;
            }
            if(context==0)im.SavePng(Path.Combine(_output,"reaction",$"frame-{f:D2}.png"));
            if(f==3||f==11)sheet.BlendRect(im,new Rect2I(0,0,64,64),new Vector2I(context*128+(f==3?0:64),0));
            if(context==1)sheet.BlendRect(im,new Rect2I(0,0,64,64),new Vector2I((f%8)*64,128+(f/8)*64));
        }
        sheet.SavePng(Path.Combine(_output,"reaction-unshaded.png"));
        bool passed=colorErrors==0&&alphaErrors==0&&endPixels==0;
        WriteJson("reaction-results.json",new {passed,frames=16,contexts=4,colorErrors,alphaErrors,endPixels,maxColorError,
            colorTolerance="one 8-bit premultiplied channel step / alpha; output alpha independently checked",
            policy="Unshaded + Mix, neutral parent, source fade only",productIntegration=false});
        view.QueueFree();
        _reactionPreview=new Sprite2D[2];
        for(int i=0;i<2;i++) {
            _reactionPreview[i]=new Sprite2D {Texture=fx.Texture,Hframes=8,Vframes=2,Frame=i*8,Position=new Vector2(375+i*140,140),Scale=Vector2.One*2,
                TextureFilter=CanvasItem.TextureFilterEnum.Nearest,Material=fx.Material};AddChild(_reactionPreview[i]);
        }
        _reactionMs=0;_reactionPhase=0;_reactionClock.Restart();
        var seen=new List<object>{new {phase=0,ms=0.0,visible=true}};_reactionTrace=seen;
        while(_reactionClock.Elapsed.TotalSeconds<1.15) {
            await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        }
        _reactionTrace=null;
        WriteJson("reaction-playback.json",new {durationsMs=_reactionDurations,lifeMs=850,previewRepeatMs=1200,transitions=seen,scope="Diagnostic repeated example; not product spawn policy"});
        return passed;
    }
}

