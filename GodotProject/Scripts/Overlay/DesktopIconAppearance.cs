using Godot;
using SiteDesktop.Platform.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SiteDesktop.Overlay;
public partial class DesktopIconAppearance : Node
{
    public static string WorkerExecutable=>System.IO.File.Exists(ProjectSettings.GlobalizePath("res://.godot/IconWorker/DesktopIconWorker.exe"))?ProjectSettings.GlobalizePath("res://.godot/IconWorker/DesktopIconWorker.exe"):System.IO.Path.Combine(OS.GetExecutablePath().GetBaseDir(),"IconWorker","DesktopIconWorker.exe");
    public sealed class Appearance : IDisposable
    {
        public DesktopIconImage Pixels { get; }
        public ImageTexture Texture { get; }
        public Vector2 Offset=>new((64-Pixels.Width)/2f,56-Pixels.Height);
        public Vector2 ZombieOffset { get; }
        public Rect2I VisibleBounds { get; }
        public int LeftRoot { get; }
        public int RightRoot { get; }
        public bool HasPixels=>VisibleBounds.Size.X>0;
        public Appearance(DesktopIconImage pixels)
        {
            Pixels=pixels;using var image=Image.CreateFromData(pixels.Width,pixels.Height,false,Image.Format.Rgba8,pixels.Rgba);
            Texture=ImageTexture.CreateFromImage(image);
            int minX=pixels.Width,minY=pixels.Height,maxX=-1,maxY=-1;
            for(int y=0;y<pixels.Height;y++)for(int x=0;x<pixels.Width;x++)if(pixels.Rgba[(y*pixels.Width+x)*4+3]>0)
            {minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);}
            if(maxX<0){VisibleBounds=new();ZombieOffset=Offset;return;}
            VisibleBounds=new(minX,minY,maxX-minX+1,maxY-minY+1);
            ZombieOffset=new((int)Math.Floor(32-(minX+VisibleBounds.Size.X/2.0)+.5),56-(maxY+1));
            var support=new List<int>();
            for(int x=0;x<64;x++)for(int y=52;y<56;y++)
            {int sx=x-(int)ZombieOffset.X,sy=y-(int)ZombieOffset.Y;if(sx>=0&&sy>=0&&sx<pixels.Width&&sy<pixels.Height&&pixels.Rgba[(sy*pixels.Width+sx)*4+3]>0){support.Add(x);break;}}
            int half=Math.Clamp((int)Math.Floor(VisibleBounds.Size.X*.18+.5),3,9);
            LeftRoot=support.OrderBy(x=>Math.Abs(x-(32-half))*100+Math.Abs(x-32)).First();
            var distinct=support.Where(x=>Math.Abs(x-LeftRoot)>=2).ToArray();
            RightRoot=(distinct.Length>0?distinct:support.ToArray()).OrderBy(x=>Math.Abs(x-(32+half))*100+Math.Abs(x-32)).First();
        }
        public bool Hit(Vector2 local,bool broken=false,bool zombie=false)
        {
            if(zombie)local+=Offset-ZombieOffset;
            if(broken)
            {
                int half=Pixels.Height/2;
                var top=Offset+new Vector2(-4,-2);var bottom=Offset+new Vector2(4,half+2);
                if(new Rect2(top,new(Pixels.Width,half)).HasPoint(local))local=local-top+Offset;
                else if(new Rect2(bottom,new(Pixels.Width,Pixels.Height-half)).HasPoint(local))local=local-bottom+Offset+new Vector2(0,half);
                else return false;
            }
            int x=(int)Math.Floor(local.X-Offset.X),y=(int)Math.Floor(local.Y-Offset.Y);
            return x>=0&&y>=0&&x<Pixels.Width&&y<Pixels.Height&&Pixels.Rgba[(y*Pixels.Width+x)*4+3]>=16;
        }
        public void Dispose()=>Texture.Dispose();
    }
    public string Status { get; private set; }="Loading desktop icons";
    public int CatalogCount=>_catalog.Length;
    public int CachedCount=>_cache.Count;
    public int SuccessfulRefreshes { get; private set; }
    public int FailedRefreshes { get; private set; }
    public Func<CancellationToken,Task<DesktopIconImage[]>>? ReadImagesAsync { get; init; }
    public bool Paused { get; private set; }
    public bool Stopped { get; private set; }
    private string[] _catalog=Array.Empty<string>();
    private readonly Dictionary<string,Appearance> _cache=new();
    private Task<DesktopIconImage[]>? _pending;
    private CancellationTokenSource? _cancel;
    private double _age=30;
    private int _next;
    private int _generation,_pendingGeneration;
    public override void _Process(double delta)
    {
        if(Stopped)return;
        if(_pending?.IsCompleted==true)
        {
            try{var images=_pending.GetAwaiter().GetResult();if(!Paused&&_pendingGeneration==_generation)Accept(images);}
            catch(OperationCanceledException){Status=Paused?"Paused":"Icon refresh cancelled";}
            catch(Exception){FailedRefreshes++;Status="Desktop icons unavailable; retrying";if(!Paused&&_pendingGeneration==_generation)_catalog=Array.Empty<string>();}
            finally{_pending=null;_cancel?.Dispose();_cancel=null;}
        }
        if(Paused)return;
        _age+=delta;
        if(_pending==null&&_age>=30){_age=0;_pendingGeneration=_generation;_cancel=new();_pending=ReadImagesAsync!=null?ReadImagesAsync(_cancel.Token):DesktopIconProvider.ReadAsync(WorkerExecutable,_cancel.Token);}
    }
    private void Accept(DesktopIconImage[] images)
    {
        foreach(var pixels in images)if(!_cache.ContainsKey(pixels.Id))_cache.Add(pixels.Id,new(pixels));
        _catalog=images.Select(i=>i.Id).Distinct().ToArray();SuccessfulRefreshes++;
        Status=$"Desktop icons: {_catalog.Length}";
    }
    public string? Choose()=>!Paused&&!Stopped&&_catalog.Length>0?_catalog[(_next++&int.MaxValue)%_catalog.Length]:null;
    public Appearance? Find(string? id)=>id!=null&&_cache.TryGetValue(id,out var image)?image:null;
    public void Retain(IEnumerable<string?> live)
    {
        var keep=new HashSet<string>(_catalog);foreach(var id in live)if(id!=null)keep.Add(id);
        foreach(var id in _cache.Keys.Where(id=>!keep.Contains(id)).ToArray()){_cache[id].Dispose();_cache.Remove(id);}
    }
    public void SetPaused(bool paused)
    {
        if(Stopped||Paused==paused)return;Paused=paused;_generation++;
        // Cancellation is observed by the process boundary; no old generation is applied.
        if(paused){_cancel?.Cancel();Status="Paused";}else _age=30;
    }
    public void Shutdown()
    {
        if(Stopped)return;Stopped=true;_cancel?.Cancel();
        if(_pending!=null)_=_pending.ContinueWith(t=>{_ = t.Exception;_cancel?.Dispose();},TaskScheduler.Default);
        else _cancel?.Dispose();
        foreach(var item in _cache.Values)item.Dispose();_cache.Clear();_catalog=Array.Empty<string>();
    }
    public override void _ExitTree()=>Shutdown();
}
