using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SiteDesktop.Diagnostics;

// Isolated render experiment. The application entry point does not depend on this scene.
public partial class Scp999LightingProbe : Node
{
    private SubViewport _viewport = null!;
    private Sprite2D _sprite = null!;
    private CanvasModulate _ambient = null!;
    private PointLight2D _light = null!;
    private CanvasTexture _material = null!;
    private Image _color = null!;
    private Texture2D _diffuse = null!, _reference = null!;
    private Label _label = null!;
    private readonly List<Window> _windows = new();
    private readonly Dictionary<string, Image> _renders = new();
    private string _revision = "r002", _output = "";
    private string _mode = "lit";
    private int _count, _frame, _direction, _loops, _tagIndex;
    private readonly List<(string Name,int From,int To,bool Loop)> _tags=new();
    private int[] _durations = Array.Empty<int>();
    private readonly Dictionary<(int Frame,bool Flip),Vector2[]> _contours = new();
    private bool _preview, _playing, _night, _flipped;
    private double _elapsed;
    private readonly Stopwatch _animationClock=new();
    private double _previousAnimationMs;
    private List<object>? _playbackEvents;
    private void ResetAnimationClock(){_animationClock.Restart();_previousAnimationMs=0;}
    private readonly Vector2[] _directions = { new(-100,-65), new(100,-65), new(0,-130) };
    private readonly string[] _names = { "left", "right", "top" };
    private int _bodyClicks, _backgroundClicks;

    public override async void _Ready()
    {
        try
        {
            var args = OS.GetCmdlineUserArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--revision") _revision = args[++i];
                else if (args[i] == "--output") _output = args[++i];
                else if (args[i] == "--preview") _preview = true;
            }
            if (DisplayServer.GetName() == "headless") throw new InvalidOperationException("Real rendering requires a Windows display.");
            if (string.IsNullOrEmpty(_output)) throw new InvalidOperationException("Specify a new --output directory.");
            if (File.Exists(Path.Combine(_output,"results.json"))) throw new IOException("Refusing to overwrite an existing result.");
            Directory.CreateDirectory(_output);
            Build();
            if (!args.Contains("--measure-only")&&!args.Contains("--playback-only"))
            {
            await CaptureCases();
            var checks = Analyze();
            if (_outline != null) await VerifyOutlineAndReaction();
            WriteJson("results.json", new { revision=_revision, engine=Engine.GetVersionInfo()["string"].AsString(), display=DisplayServer.GetName(), renderer=RenderingServer.GetCurrentRenderingMethod(), frames=_count, checks, settings=new { lightHeight=100, dayEnergy=0.72, nightEnergy=0.35, dayAmbient=0.42, nightAmbient=new[]{0.40,0.40,0.48}, shininess=0.45, specularColor=0.35, nearest=true, size=128, normalFlip="Sprite2D.FlipH, no manual channel inversion", viewportAlpha="premultiplied display; straight RGBA evidence" }, userApproval="pending", productIntegration=false });
            GD.Print("LIGHTING_CAPTURE_COMPLETE " + _output);
            if (_count >= 12) await CapturePlayback();
            await CheckWindows();
            }
            if(args.Contains("--playback-only"))
            {
                for(int f=0;f<_count;f++){SetState(f,0,false,false,"lit");_renders[Key(f,0,false,false,"lit")]=await Capture();}
                if(_outline!=null&&!await VerifyReaction())throw new InvalidDataException("Reaction verification failed.");
                await CapturePlayback();await CheckWindows();
                WriteJson("playback-scope.json",new {revision=_revision,frames=_count,scope="Fresh left/day renders plus six-condition real-time playback; full lighting matrix is separate evidence"});
                GD.Print("LIGHTING_CAPTURE_COMPLETE playback-only "+_output);
            }
            if (args.Contains("--measure") || args.Contains("--measure-only")) await Measure();
            if (args.Contains("--measure-only")) GD.Print("LIGHTING_CAPTURE_COMPLETE measurement-only " + _output);
            SetState(0,0,false,false,"lit");
            _playing = _count >= 12; ResetAnimationClock(); _elapsed=0; _loops=0; _tagIndex=0;
            _label.Text = $"{_revision} | arrows: light | N: night | F: flip | Space: play | H: hide | Esc: exit\n" +
                "Two transparent windows share this rendered texture. Click body / surrounding test panel.\n" + _output;
            if (!_preview) GetTree().Quit();
        }
        catch (Exception ex) { GD.PushError("LIGHTING_FAILED " + ex); GetTree().Quit(1); }
    }

    private void Build()
    {
        string path=$"res://Assets/Diagnostics/Scp999Lighting/{_revision}/";
        _color=Image.LoadFromFile(ProjectSettings.GlobalizePath(path+"Scp999_color.png"));
        bool split=File.Exists(ProjectSettings.GlobalizePath(path+"Scp999_outline.png"));
        if(split) _color=Image.LoadFromFile(ProjectSettings.GlobalizePath(path+"Scp999_combined.png"));
        _count=_color.GetWidth()/64;
        using var doc=JsonDocument.Parse(File.ReadAllText(ProjectSettings.GlobalizePath(path+"Scp999.contract.json")));
        _durations=doc.RootElement.GetProperty("frames").EnumerateArray().Select(f=>f.GetProperty("durationMs").GetInt32()).ToArray();
        foreach(var tag in doc.RootElement.GetProperty("tags").EnumerateArray())_tags.Add((tag.GetProperty("name").GetString()!,tag.GetProperty("from").GetInt32(),tag.GetProperty("to").GetInt32(),tag.GetProperty("loop").GetBoolean()));
        if (_count!=_durations.Length || _color.GetHeight()!=64) throw new InvalidDataException("Sheet/contract mismatch.");
        // Build each input contour once. Rebuilding native Image/Bitmap resources every animation
        // frame grows working set until managed finalizers run and wastes CPU in the render test.
        for(int frame=0;frame<_count;frame++)
        {
            using var region=_color.GetRegion(new Rect2I(frame*64,0,64,64));
            using var bitmap=new Godot.Bitmap();bitmap.CreateFromImageAlpha(region,.1f);
            var polygons=bitmap.OpaqueToPolygons(new Rect2I(0,0,64,64),0);
            if(polygons.Count==0)throw new InvalidDataException("Empty character contour.");
            var polygon=polygons.OrderByDescending(p=>p.Length).First();
            foreach(bool flip in new[]{false,true})_contours[(frame,flip)]=polygon.Select(p=>new Vector2(flip?128-p.X*2:p.X*2,p.Y*2)).ToArray();
        }
        // Load PNGs as raw lossless ImageTextures: no importer gamma conversion, mipmaps or compression.
        _diffuse=ImageTexture.CreateFromImage(_color);
        _reference=ImageTexture.CreateFromImage(Image.LoadFromFile(ProjectSettings.GlobalizePath(path+(split?"Scp999_combined.png":"r001-reference.png"))));
        _material=new CanvasTexture { DiffuseTexture=_diffuse,
            NormalTexture=ImageTexture.CreateFromImage(Image.LoadFromFile(ProjectSettings.GlobalizePath(path+"Scp999_normal.png"))),
            SpecularTexture=ImageTexture.CreateFromImage(Image.LoadFromFile(ProjectSettings.GlobalizePath(path+"Scp999_specular.png"))),
            SpecularShininess=.45f, SpecularColor=new Color(.35f,.35f,.35f),
            TextureFilter=CanvasItem.TextureFilterEnum.Nearest, TextureRepeat=CanvasItem.TextureRepeatEnum.Disabled };
        _viewport=new SubViewport { Size=new Vector2I(128,128), TransparentBg=true, Disable3D=true,
            RenderTargetUpdateMode=SubViewport.UpdateMode.Always, OwnWorld3D=false };
        AddChild(_viewport);
        _sprite=new Sprite2D { Texture=_material, Hframes=_count, Position=new Vector2(64,64), Scale=new Vector2(2,2), TextureFilter=CanvasItem.TextureFilterEnum.Nearest };
        _viewport.AddChild(_sprite);
        if(split) BuildOutline(path);
        _ambient=new CanvasModulate(); _viewport.AddChild(_ambient);
        var lightImage=Image.CreateEmpty(512,512,false,Image.Format.Rgba8); lightImage.Fill(Colors.White);
        _light=new PointLight2D { Texture=ImageTexture.CreateFromImage(lightImage), Height=100, ShadowEnabled=false };
        _viewport.AddChild(_light);
        RenderingServer.ViewportSetMeasureRenderTime(_viewport.GetViewportRid(),true);
        GetWindow().Title="SCP-999 lighting laboratory"; GetWindow().Size=new Vector2I(900,600);
        var screenOrigin=DisplayServer.ScreenGetUsableRect(DisplayServer.GetPrimaryScreen()).Position;
        GetWindow().Position=screenOrigin+new Vector2I(80,100);
        var bg=new ColorRect { Color=new Color(.13f,.16f,.20f), Size=new Vector2(900,600) };
        AddChild(bg); bg.GuiInput+=e=> { if(e is InputEventMouseButton {Pressed:true,ButtonIndex:MouseButton.Left}) { _backgroundClicks++; GD.Print("LIGHTING_BACKGROUND_CLICK "+_backgroundClicks); } };
        _label=new Label { Position=new Vector2(20,15), Text="Capturing actual Godot lighting..." }; AddChild(_label);
        var displayMaterial=new CanvasItemMaterial { BlendMode=CanvasItemMaterial.BlendModeEnum.PremultAlpha, LightMode=CanvasItemMaterial.LightModeEnum.Unshaded };
        var view=new TextureRect { Texture=_viewport.GetTexture(), Material=displayMaterial, Position=new Vector2(320,130), Size=new Vector2(256,256), TextureFilter=CanvasItem.TextureFilterEnum.Nearest, MouseFilter=Control.MouseFilterEnum.Ignore };
        AddChild(view);
        var exit=new Button { Text="Exit",Position=new Vector2(790,550),Size=new Vector2(90,35) }; AddChild(exit);exit.Pressed+=()=>GetTree().Quit();
        for(int i=0;i<2;i++)
        {
            var window=new Window { Title="SCP lighting sample "+(i+1), Visible=false, ForceNative=true, Transient=false,
                Borderless=true, Transparent=true, TransparentBg=true, AlwaysOnTop=true, Unresizable=true,
                Unfocusable=true, InitialPosition=Window.WindowInitialPosition.Absolute, Size=new Vector2I(128,128), Position=screenOrigin+new Vector2I(180+i*500,490) };
            AddChild(window);
            window.AddChild(new TextureRect { Texture=_viewport.GetTexture(),Material=displayMaterial,Size=new Vector2(128,128),TextureFilter=CanvasItem.TextureFilterEnum.Nearest,MouseFilter=Control.MouseFilterEnum.Ignore });
            window.WindowInput+=e=>{ if(e is InputEventMouseButton {Pressed:true,ButtonIndex:MouseButton.Left}) { _bodyClicks++; GD.Print("LIGHTING_BODY_CLICK "+_bodyClicks); } };
            _windows.Add(window); window.Show();
        }
    }

    private void SetState(int frame,int direction,bool flip,bool night,string mode)
    {
        _frame=frame; _direction=direction; _night=night; _flipped=flip;
        _mode=mode;
        _sprite.Frame=frame; _sprite.FlipH=flip;
        _sprite.Texture=mode=="reference" ? _reference : mode=="base" ? _diffuse : _material;
        _material.SpecularShininess=.45f;
        _material.SpecularColor=mode=="normal" ? Colors.Black : new Color(.35f,.35f,.35f);
        _ambient.Color=(mode=="base"||mode=="reference") ? Colors.White : new Color(night?.40f:.42f,night?.40f:.42f,night?.48f:.42f);
        _light.Enabled=mode=="lit"||mode=="normal"||mode=="legacy";
        _light.Energy=night?.35f:.72f;
        _light.Color=night?new Color(.70f,.80f,1):new Color(1,.95f,.88f);
        _light.Position=new Vector2(64,64)+_directions[direction];
        ApplyOutlineState(frame,flip,mode);
        foreach(var window in _windows) window.MousePassthroughPolygon=_contours[(frame,flip)];
        _playbackEvents?.Add(new {direction,flip,frame,ms=_animationClock.Elapsed.TotalMilliseconds});
    }

    private async Task<Image> Capture()
    {
        for(int i=0;i<3;i++) await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
        var image=_viewport.GetTexture().GetImage(); image.Convert(Image.Format.Rgba8);
        // Transparent Viewport readback is premultiplied. PNG/review compositing needs straight RGBA.
        for(int y=0;y<128;y++) for(int x=0;x<128;x++)
        {
            var c=image.GetPixel(x,y);if(c.A>0)image.SetPixel(x,y,new Color(c.R/c.A,c.G/c.A,c.B/c.A,c.A));
        }
        return image;
    }
    private static string Key(int frame,int direction,bool flip,bool night,string mode)=>$"f{frame:D2}_{direction}_{flip}_{night}_{mode}";
    private async Task CaptureCases()
    {
        foreach(bool night in new[]{false,true}) foreach(bool flip in new[]{false,true})
        for(int direction=0;direction<3;direction++) for(int frame=0;frame<_count;frame++)
        {
            foreach(string mode in new[]{"base","normal","lit"})
            {
                SetState(frame,direction,flip,night,mode);
                var image=await Capture();_renders[Key(frame,direction,flip,night,mode)]=image;
            }
        }
        for(int frame=0;frame<_count;frame++) { SetState(frame,0,false,false,"reference"); _renders[Key(frame,0,false,false,"reference")]=await Capture(); }
        foreach(bool night in new[]{false,true})
        {
            var board=Image.CreateEmpty(_count*128,6*128,false,Image.Format.Rgba8);board.Fill(new Color(.17f,.19f,.23f));
            for(int frame=0;frame<_count;frame++) for(int row=0;row<6;row++)
                board.BlendRect(_renders[Key(frame,row%3,row>=3,night,"lit")],new Rect2I(0,0,128,128),new Vector2I(frame*128,row*128));
            board.SavePng(Path.Combine(_output,night?"lighting-night.png":"lighting-day.png"));
        }
        var comparison=Image.CreateEmpty(_count*128,4*128,false,Image.Format.Rgba8);comparison.Fill(new Color(.17f,.19f,.23f));
        var modes=new[]{"reference","base","normal","lit"};
        for(int row=0;row<4;row++) for(int frame=0;frame<_count;frame++) comparison.BlendRect(_renders[Key(frame,0,false,false,modes[row])],new Rect2I(0,0,128,128),new Vector2I(frame*128,row*128));
        comparison.SavePng(Path.Combine(_output,"mode-comparison.png"));
        foreach(var tag in _tags)
        {
            var strip=Image.CreateEmpty((tag.To-tag.From+1)*128,4*128,false,Image.Format.Rgba8);strip.Fill(new Color(.17f,.19f,.23f));
            for(int row=0;row<4;row++)for(int frame=tag.From;frame<=tag.To;frame++)strip.BlendRect(_renders[Key(frame,0,false,false,modes[row])],new Rect2I(0,0,128,128),new Vector2I((frame-tag.From)*128,row*128));
            strip.SavePng(Path.Combine(_output,$"mode-{tag.Name}.png"));
        }
        var overview=Image.CreateEmpty(640,128*((_tags.Count+4)/5),false,Image.Format.Rgba8);overview.Fill(new Color(.17f,.19f,.23f));
        for(int tag=0;tag<_tags.Count;tag++)overview.BlendRect(_renders[Key(Math.Min(_tags[tag].From+1,_tags[tag].To),0,false,false,"lit")],new Rect2I(0,0,128,128),new Vector2I(tag%5*128,tag/5*128));
        overview.SavePng(Path.Combine(_output,"actions-overview.png"));
        var backgrounds=Image.CreateEmpty(384,256,false,Image.Format.Rgba8);
        for(int y=0;y<256;y++) for(int x=0;x<384;x++) backgrounds.SetPixel(x,y,x<128?new Color(.95f,.94f,.91f):x<256?new Color(.10f,.12f,.16f):(x/8+y/8)%2==0?new Color(.75f,.68f,.57f):new Color(.49f,.60f,.64f));
        for(int row=0;row<2;row++)for(int col=0;col<3;col++) backgrounds.BlendRect(_renders[Key(0,0,false,row==1,"lit")],new Rect2I(0,0,128,128),new Vector2I(col*128,row*128));
        backgrounds.SavePng(Path.Combine(_output,"backgrounds.png"));
    }

    private object Analyze()
    {
        int alphaErrors=0; double flipError=0, specDelta=0; var directions=new List<object>();
        for(int frame=0;frame<_count;frame++)
        {
            var left=_renders[Key(frame,0,false,false,"lit")];var right=_renders[Key(frame,1,false,false,"lit")];
            var mirror=_renders[Key(frame,1,true,false,"lit")];var noSpec=_renders[Key(frame,0,false,false,"normal")];
            var basis=_renders[Key(frame,0,false,false,"base")];
            double lx=0,rx=0,lw=0,rw=0;
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                var a=left.GetPixel(x,y);var b=right.GetPixel(x,y);var m=mirror.GetPixel(127-x,y);
                flipError+=Math.Abs(a.R-m.R)+Math.Abs(a.G-m.G)+Math.Abs(a.B-m.B);
                specDelta+=Math.Max(0,Luma(a)-Luma(noSpec.GetPixel(x,y)));
                double wa=Math.Max(0,Luma(a)-.25)*a.A,wb=Math.Max(0,Luma(b)-.25)*b.A;
                lx+=x*wa;lw+=wa;rx+=x*wb;rw+=wb;
            }
            directions.Add(new {frame,leftBrightX=lx/Math.Max(lw,.001),rightBrightX=rx/Math.Max(rw,.001)});
        }
        foreach(bool night in new[]{false,true}) foreach(bool flip in new[]{false,true})
        for(int direction=0;direction<3;direction++) for(int frame=0;frame<_count;frame++)
        {
            var basis=_renders[Key(frame,direction,flip,night,"base")];var lit=_renders[Key(frame,direction,flip,night,"lit")];
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)if(Math.Abs(lit.GetPixel(x,y).A-basis.GetPixel(x,y).A)>1.1/255)alphaErrors++;
        }
        GD.Print("ALPHA_DIAGNOSTIC source="+_color.GetPixel(32,30)+" rendered="+_renders[Key(0,0,false,false,"base")].GetPixel(64,60));
        return new { alphaErrors, mirrorMeanRgbError=flipError/(_count*128*128*3), positiveSpecularMean=specDelta/(_count*128*128), brightnessCentroids=directions,
            interpretation="Numeric diagnostics plus visual review required; centroid moves with shape and is not a quality score." };
    }
    private static double Luma(Color c)=>.2126*c.R+.7152*c.G+.0722*c.B;
    private void WriteJson(string name,object value)=>File.WriteAllText(Path.Combine(_output,name),JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}));

    private async Task CheckWindows()
    {
        bool native=_windows.All(w=>!w.IsEmbedded()&&w.Visible&&w.Transparent&&w.TransparentBg&&w.MousePassthroughPolygon.Length>3);
        foreach(var w in _windows)w.Hide();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        bool hidden=_windows.All(w=>!w.Visible);
        foreach(var w in _windows)w.Show();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        WriteJson("windows.json",new {native,hidden,restored=_windows.All(w=>w.Visible),sharedRenderTexture=true,inputReception="requires real click test; polygon property is not proof"});
    }
    private async Task CapturePlayback()
    {
        Directory.CreateDirectory(Path.Combine(_output,"playback"));
        var transitions=new List<object>();
        for(int i=0;i<_count;i++)
        {
            _renders[Key(i,0,false,false,"lit")].SavePng(Path.Combine(_output,"playback",$"frame-{i:D2}.png"));
            var tag=_tags.First(t=>i>=t.From&&i<=t.To);
            int next=i==tag.To&&tag.Loop?tag.From:(i+1)%_count;
            var a=_renders[Key(i,0,false,false,"lit")]; var b=_renders[Key(next,0,false,false,"lit")];
            double sum=0,max=0;int count=0;
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                var ca=a.GetPixel(x,y);var cb=b.GetPixel(x,y);
                if(ca.A<.5||cb.A<.5)continue;
                double d=Math.Abs(Luma(ca)-Luma(cb));sum+=d;max=Math.Max(max,d);count++;
            }
            transitions.Add(new {from=i,to=next,meanLuminanceDelta=sum/Math.Max(1,count),maxLuminanceDelta=max,commonPixels=count});
        }
        WriteJson("transitions.json",new {durationsMs=_durations,transitions,warning="Geometry moves; common-pixel differences are diagnostic, not automatic flicker pass/fail."});
        var seen=new List<object>();
        foreach(bool flip in new[]{false,true})for(int direction=0;direction<3;direction++)
        {
            _elapsed=0;_loops=0;_tagIndex=0;ResetAnimationClock();_playbackEvents=seen;SetState(0,direction,flip,false,"lit");_playing=true;
            double seconds=Math.Max(8,_tags.Sum(t=>_durations.Skip(t.From).Take(t.To-t.From+1).Sum()*(t.Loop?2:1))/1000.0+.6);
            while(_animationClock.Elapsed.TotalSeconds<seconds)
            {
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            }
            _playing=false;_playbackEvents=null;
        }
        _playing=false;WriteJson("playback-timing.json",seen);
    }
    private async Task Measure()
    {
        var samples=new List<object>();
        foreach(int windowCount in new[]{1,2})
        {
            if(windowCount==1)_windows[1].Hide();else _windows[1].Show();
            for(int repeat=0;repeat<3;repeat++) foreach(string mode in _outline==null?new[]{"base","lit"}:new[]{"legacy","lit"})
            {
                SetState(0,0,false,false,mode);_elapsed=0;_loops=0;_tagIndex=0;ResetAnimationClock();_playing=_count>=12;
                await ToSignal(GetTree().CreateTimer(2),SceneTreeTimer.SignalName.Timeout);
                var cpu=new List<double>();var gpu=new List<double>();var intervals=new List<double>();
                using var process=Process.GetCurrentProcess();var before=process.TotalProcessorTime;long memoryBefore=process.WorkingSet64;
                var timer=Stopwatch.StartNew();double previous=0;
                while(timer.Elapsed.TotalSeconds<10)
                {
                    await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                    double now=timer.Elapsed.TotalMilliseconds;intervals.Add(now-previous);previous=now;
                    cpu.Add(RenderingServer.ViewportGetMeasuredRenderTimeCpu(_viewport.GetViewportRid()));
                    gpu.Add(RenderingServer.ViewportGetMeasuredRenderTimeGpu(_viewport.GetViewportRid()));
                }
                process.Refresh();samples.Add(new {windowCount,mode,repeat,samples=cpu.Count,cpuRenderP95Ms=P95(cpu),gpuRenderP95Ms=P95(gpu),frameP95Ms=P95(intervals),processCpuOneCorePercent=(process.TotalProcessorTime-before).TotalSeconds/timer.Elapsed.TotalSeconds*100,workingSetBytes=process.WorkingSet64,memoryBefore});
                GD.Print($"LIGHTING_MEASURE windows={windowCount} mode={mode} repeat={repeat}");
                _playing=false;
            }
        }
        WriteJson("performance.json",new {warmupSeconds=2,measurementSeconds=10,repeats=3,order=_outline==null?"alternating base/lit":"alternating r004 legacy lit/r005 split lit",animation=_count>=12,vsync="enabled",maxFps=60,sharedSubViewport=true,samples});
    }
    private static double P95(List<double> values){values.Sort();return values[(int)Math.Floor((values.Count-1)*.95)];}
    public override void _Process(double delta)
    {
        UpdateReactionPreview(delta);
        if(!_playing)return;
        // Diagnostic playback follows real milliseconds, independent of engine delta smoothing
        // after long image capture/analysis work. Restart on each playback context/resume.
        double now=_animationClock.Elapsed.TotalMilliseconds;
        _elapsed+=now-_previousAnimationMs;_previousAnimationMs=now;
        while(_elapsed>=_durations[_frame])
        {
            _elapsed-=_durations[_frame];int next=_frame+1;
            var tag=_tags[_tagIndex];
            if(_frame==tag.To)
            {
                _loops++;
                if(tag.Loop&&_loops<2)next=tag.From;
                else{_loops=0;_tagIndex=(_tagIndex+1)%_tags.Count;next=_tags[_tagIndex].From;}
            }
            SetState(next,_direction,_flipped,_night,_mode);
        }
    }
    public override void _UnhandledKeyInput(InputEvent e)
    {
        if(e is not InputEventKey {Pressed:true,Echo:false} key)return;
        if(key.Keycode==Godot.Key.Escape){GetTree().Quit();return;}
        if(key.Keycode==Godot.Key.Space){_playing=!_playing&&_count>=12;ResetAnimationClock();}
        if(key.Keycode==Godot.Key.N)_night=!_night;
        if(key.Keycode==Godot.Key.F)_flipped=!_flipped;
        if(key.Keycode==Godot.Key.Left)_direction=0;
        if(key.Keycode==Godot.Key.Right)_direction=1;
        if(key.Keycode==Godot.Key.Up)_direction=2;
        if(key.Keycode==Godot.Key.H)foreach(var w in _windows)w.Visible=!w.Visible;
        SetState(_frame,_direction,_flipped,_night,"lit");
    }
}
