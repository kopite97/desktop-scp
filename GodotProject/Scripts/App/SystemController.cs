using Godot;
using SiteDesktop.Overlay;
using SiteDesktop.Platform.Windows;
using SiteDesktop.Simulation;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SiteDesktop.App;

public partial class SystemController : Node
{
    public Main App { get; init; }=null!;
    public SystemOptions State { get; }=new();
    public bool AutomaticMetrics { get; set; }=true;
    public bool PlayAlarm { get; set; }=true;
    public SiteTray? Tray { get; private set; }
    public LockdownHotkey Hotkey { get; }=new();
    public WindowsWindowShake Shake { get; private set; }=null!;
    public SystemCueWindow? Cue { get; private set; }
    public int Samples { get; private set; }
    public double MaxSampleMilliseconds { get; private set; }
    public bool Sampling => _pending!=null;
    private readonly WindowsSystemMetrics _metrics=new();
    private Task<(SystemSample Sample,double Milliseconds)>? _pending;
    private long _generation;
    private double _sampleTime=1,_cueTime,_shutter=-1;
    private AudioStreamPlayer? _alarm;
    public override void _Ready()
    {
        Shake=new(State);
        if(DisplayServer.GetName()=="headless")return;
        try { Tray=App.TrayFactory(ToggleLockdown,App.ShowControls,App.RequestExit); }
        catch(Exception ex) { GD.PushWarning("Tray unavailable: "+ex.Message); }
        Tray?.Update(false);Hotkey.Start();
        Cue=new();AddChild(Cue);
        _alarm=new AudioStreamPlayer{VolumeDb=-24};
        byte[] pcm=new byte[16000/5*2];
        for(int i=0;i<pcm.Length/2;i++)
        {double t=i/16000d;short s=(short)(Math.Sin(t*Math.Tau*(t<.1?680:480))*15000*Math.Sin(Math.PI*t/.2));pcm[i*2]=(byte)s;pcm[i*2+1]=(byte)(s>>8);}
        _alarm.Stream=new AudioStreamWav{Format=AudioStreamWav.FormatEnum.Format16Bits,MixRate=16000,Data=pcm};AddChild(_alarm);
    }
    public void SetStrong(bool enabled){State.SetStrong(enabled);if(!enabled)Shake.Cancel();}
    public void ToggleLockdown()=>SetLocked(!State.Locked);
    public void SetLocked(bool locked)
    {
        if(!State.SetLocked(locked))return;
        Shake.Cancel();
        App.Scps.RefreshLockdown();
        Tray?.Update(locked);App.UpdateLockButton(locked);
        if(locked)
        {
            _shutter=0;
            if(PlayAlarm)_alarm?.Play();
            App.HideControls();
        }
        else
        { _shutter=-1;_sampleTime=1;Cue?.Hide();_alarm?.Stop();ApplyReactions(0); }
        GD.Print(locked?"SITE_LOCKED":"SITE_RESUMED");
    }
    public bool Accept(SystemSample sample,long generation)
    {bool accepted=State.Accept(sample,generation);if(accepted){Samples++;ApplyReactions(0);}return accepted;}
    public void ApplyReactions(double delta)
    {
        if(State.Stopped||State.Locked)return;
        App.Scps.ApplyReactions(State,delta);
    }
    private string? _effectOwner;
    public void ActivationChanged()
    {
        if(_effectOwner!=null&&!App.Scps.IsActive(_effectOwner)){Shake.Cancel();_effectOwner=null;}
        ApplyReactions(0);
    }
    public bool RequestStressEffect()
    {
        if(State.Stopped||State.Locked||App.Scps.EffectSource is not { } source||!Shake.StartForeground())return false;
        _effectOwner=source.Definition.Id;return true;
    }
    public override void _Process(double delta)
    {
        if(State.Stopped)return;
        if(Hotkey.Consume())ToggleLockdown();
        State.Tick(delta);_cueTime+=delta;
        if(_pending?.IsCompleted==true)
        {
            if(_pending.IsCompletedSuccessfully){MaxSampleMilliseconds=Math.Max(MaxSampleMilliseconds,_pending.Result.Milliseconds);Accept(_pending.Result.Sample,_generation);}
            else { _ = _pending.Exception;Accept(default,_generation); }
            _pending=null;
        }
        if(AutomaticMetrics&&!State.Locked&&DisplayServer.GetName()!="headless"&&(_sampleTime+=delta)>=1&&_pending==null)
        {_sampleTime=0;_generation=State.Generation;_pending=Task.Run(()=>{var timer=System.Diagnostics.Stopwatch.StartNew();var sample=_metrics.Read();return(sample,timer.Elapsed.TotalMilliseconds);});}
        ApplyReactions(delta);Shake.Tick(delta);
        RequestStressEffect();
        if(Cue!=null)
        {
            if(_shutter>=0)
            {_shutter+=delta;if(_shutter<SystemCueWindow.LockdownCueSeconds)Cue.Publish(App.Companion.State.Screens[0].Usable,true,(float)_shutter,false);else{Cue.Hide();_shutter=-1;}}
            else if(!State.Locked&&App.Scps.Entries.Any(e=>e.Active)&&(State.Hot||State.Noisy))Cue.Publish(App.Companion.State.Screens[0].Usable,false,(float)_cueTime,State.Noisy);
            else Cue.Hide();
            App.Stack.Update("system-cues",Cue.Visible?new[]{new OverlayStackCoordinator.Entry(Cue,50,0,false)}:Array.Empty<OverlayStackCoordinator.Entry>());
        }
    }
    public void Shutdown()
    {
        if(State.Stopped)return;State.Shutdown();Shake.Dispose();Hotkey.Dispose();Tray?.Dispose();
        App.Stack.Remove("system-cues");Cue?.Hide();Cue?.QueueFree();Cue=null;_alarm?.Stop();
        // No queued sampling: the sole in-flight read releases its COM pointers in its own finally.
        if(_pending!=null)_=_pending.ContinueWith(t=>{if(t.IsFaulted)_=t.Exception;},TaskScheduler.Default);
        _pending=null;GD.Print("SYSTEM_OPTIONS_STOPPED");
    }
    public override void _ExitTree()=>Shutdown();
}
