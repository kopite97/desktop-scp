using Godot;
using SiteDesktop.Overlay;
using SiteDesktop.Simulation;
using System;
using System.Linq;
using SiteDesktop.App.Scps;
using SiteDesktop.Platform.Windows;

namespace SiteDesktop.App;

public partial class Main : Control
{
    private Button? _exitButton;
    private Button? _visibilityButton;
    private Button? _resetButton;
    public Scp999Controller Companion=>_creation.Find<Scp999Controller>("scp-999")!;
    public bool EnablePlague { get; set; }=true;
    public bool UseDesktopIcons { get; set; }=true;
    public bool EnableStatue { get; set; }=true;
    public bool EnableSystemOptions { get; set; }=true;
    public SystemController? Systems { get; private set; }
    private CheckButton? _strongButton;
    public StatueController? Statue=>_creation.Find<StatueController>("scp-173");
    public DesktopIconAppearance? Icons { get; private set; }
    public PlagueController? Plague=>_creation.Find<PlagueController>("scp-049");
    public DesktopObjectController? Objects { get; private set; }
    public WindowsDesktopEnvironmentSource Environment { get; private set; }=null!;
    private ScpCreationContext _creation=null!;
    public ScpRuntime Runtime { get; private set; }=null!;
    public ScpActivationManager Scps { get; private set; }=null!;
    public ScpSettingsStore SettingsStore { get; private set; }=null!;
    public ScpOptionsPanel OptionsPanel { get; private set; }=null!;
    public bool PersistScpSettings { get; set; }=true;
    public string? SettingsPath { get; set; }
    private bool _diagnosticLocked;
    private Button? _spawnButton;
    public StressState Stress { get; } = new();
    public OverlayStackCoordinator Stack { get; } = new();
    private double _statusTime;
    private bool _exiting;
    public Func<Action, Action, Action, SiteTray> TrayFactory { get; set; } = (toggle, controls, exit) => new(toggle, controls, exit);
    public bool CanHideControls => Systems?.Tray?.Available == true;

    public override void _EnterTree()
    {
        if (EnableSystemOptions) WindowStartup.KeepHidden = true;
    }

    public override void _Ready()
    {
        GetNode<Label>("Scroll/Content/RuntimeStatus").Text = "C# runtime ready";
        _exitButton = GetNode<Button>("Scroll/Content/Exit");
        _exitButton.Pressed += ControlButtonPressed;
        _visibilityButton = GetNode<Button>("Scroll/Content/Visibility");
        _visibilityButton.Pressed += ToggleOverlays;
        _resetButton = GetNode<Button>("Scroll/Content/Reset");
        _resetButton.Pressed += ResetOverlays;
        GetTree().AutoAcceptQuit = false;
        GetWindow().CloseRequested += HideControls;
        AddChild(Stack);
        var args=OS.GetCmdlineUserArgs();
        for(int i=0;i+1<args.Length;i++)if(args[i]=="--scp-settings-file")SettingsPath=args[++i];
        Environment=new();Runtime=new(){Environment=Environment};
        _creation=new(node=>AddChild(node),Runtime,Environment,Stress,Stack);
        var catalog=ScpCatalog.Create(_creation,EnablePlague,EnableStatue);
        SettingsStore=new(PersistScpSettings?SettingsPath??ProjectSettings.GlobalizePath("user://scp-settings.json"):null);
        var settings=SettingsStore.Load();
        bool Enabled(ScpDefinition d)=>settings.TryGetValue(d.Id,out bool value)?value:d.DefaultEnabled;
        if(catalog.Any(d=>d.UsesObjects))
        {
            bool active=catalog.Any(d=>d.UsesObjects&&Enabled(d));
            if(UseDesktopIcons){Icons=new();AddChild(Icons);Icons.SetPaused(!active);}
            Objects=new(){Environment=Environment,Stack=Stack,Icons=Icons,InitialActive=active,InitialZombiesActive=catalog.Any(d=>d.OwnsZombies&&Enabled(d))};AddChild(Objects);
        }
        _creation.Objects=Objects;
        if(Objects!=null)Runtime.Register(Objects.RuntimeTick);
        Scps=new(catalog,settings,()=>Systems?.State.Locked??_diagnosticLocked);
        if(Objects!=null)
        {
            Objects.State.OwnerActive=Scps.IsOwnerActive;
            Objects.State.SpawnAnchorProvider=SpawnAnchor;
            Objects.SpawnData();
        }
        AddChild(Runtime);
        Scps.Changed+=ActivationChanged;
        OptionsPanel=new(){Manager=Scps,Store=SettingsStore};GetNode<VBoxContainer>("Scroll/Content").AddChild(OptionsPanel);
        GetNode<VBoxContainer>("Scroll/Content").MoveChild(OptionsPanel,2);
        _spawnButton=new Button{Text="Spawn icon clone",CustomMinimumSize=new(0,38),Disabled=Objects==null};
        GetNode<VBoxContainer>("Scroll/Content").AddChild(_spawnButton);_spawnButton.Pressed+=SpawnData;
        if(EnableSystemOptions)
        {
            Systems=new(){App=this};AddChild(Systems);
            _strongButton=new(){Text="Allow strong effects (move foreground window)",CustomMinimumSize=new(0,38)};
            GetNode<VBoxContainer>("Scroll/Content").AddChild(_strongButton);_strongButton.Toggled+=StrongChanged;
            UpdateLockButton(false);
        }
        if (DisplayServer.GetName() != "headless")
        {
            RefreshTrayControls();
            if (CanHideControls) HideControls(); else ShowControls();
        }
        GD.Print("SITE_DESKTOP_READY");
    }

    private SpawnAnchor? SpawnAnchor()
    {
        if(Scps.IsActive("scp-049")&&Plague is {} doctor)
            return new(SiteDesktop.Desktop.ScreenLocator.Footprint(doctor.State.Doctor.Body.Foot,40,112),doctor.State.Phase==DoctorPhase.Dragging||doctor.State.Behavior.Portal.Active);
        if(Scps.IsActive("scp-173")&&Statue is {} statue)
            return new(SiteDesktop.Desktop.ScreenLocator.Footprint(statue.State.Physics.Body.Foot,28,96),statue.State.Dragging);
        return null;
    }

    private void ToggleOverlays()
    {
        if(Systems!=null){Systems.ToggleLockdown();return;}
        _diagnosticLocked=!_diagnosticLocked;Scps.RefreshLockdown();bool show=!_diagnosticLocked;
        _visibilityButton!.Text = show ? "Hide overlays" : "Show overlays";
    }
    private void StrongChanged(bool enabled)=>Systems?.SetStrong(enabled);
    public void UpdateLockButton(bool locked){if(_visibilityButton!=null)_visibilityButton.Text=locked?"Resume (Ctrl+Shift+L)":"Lockdown (Ctrl+Shift+L)";}
    public void ShowControls()
    {
        if (_exiting || DisplayServer.GetName() == "headless") return;
        WindowStartup.PrepareControls(GetWindow(), new(640,900));
        GetWindow().Mode = Window.ModeEnum.Windowed;
        WindowsDesktop.SetControlVisible(GetWindow(), true);
        GetWindow().GrabFocus();
        GD.Print("SITE_CONTROLS_SHOWN");
    }
    public void HideControls()
    {
        if (_exiting || DisplayServer.GetName() == "headless") return;
        RefreshTrayControls();
        if (!CanHideControls) { ShowControls(); return; }
        WindowsDesktop.SetControlVisible(GetWindow(), false);
        GD.Print("SITE_CONTROLS_HIDDEN");
    }
    private void ControlButtonPressed()
    {
        if (CanHideControls) HideControls(); else RequestExit();
    }
    private void RefreshTrayControls()
    {
        _exitButton!.Text = CanHideControls ? "Hide controls (exit from tray)" : "Exit";
        GetNode<Label>("Scroll/Content/TrayStatus").Text = CanHideControls
            ? "Closing this window keeps SCPs active. Use the tray menu to exit."
            : "Tray unavailable. Keep this window open; use Exit to quit.";
    }

    private void ResetOverlays()=>Scps.Reset();
    private void SpawnData()=>Objects?.SpawnData();
    private void ActivationChanged(string? id)
    {
        if(id!=null){SettingsStore.Save(Scps.Settings);GD.Print($"SCP_SETTING {id}={Scps.Settings[id]}");}
        bool active=Scps.Entries.Any(e=>e.Active&&e.Definition.UsesObjects);
        bool zombies=Scps.Entries.Any(e=>e.Active&&e.Definition.OwnsZombies);
        bool clear=id!=null&&Scps.Entries.Any(e=>e.Definition.Id==id&&e.Definition.OwnsZombies&&!e.Enabled);
        if(id!=null&&Scps.Entries.First(e=>e.Definition.Id==id) is {Enabled:false,Definition.RuntimeOwner:{ } owner})Objects?.State.ReleaseOwner(owner);
        Objects?.SetActivity(active,zombies,clear);
        Systems?.ActivationChanged();
    }

    public override void _Process(double delta)
    {
        if((_statusTime+=delta)<.25)return;
        _statusTime=0;
        GetNode<Label>("Scroll/Content/RuntimeStatus").Text=string.Join("\n",Scps.Entries.Select(e=>$"{e.Definition.Name}: {(e.Active?e.Module.Status:e.State)}"));
        if(Icons!=null)GetNode<Label>("Scroll/Content/RuntimeStatus").Text+="\n"+Icons.Status;
        _spawnButton!.Disabled=Objects?.State.Visible!=true||(Icons!=null&&Icons.CatalogCount==0);
        if(Systems!=null)
        {
            var s=Systems.State;var m=s.Sample;
            static string Percent(float? p)=>p is float v?$"{v:0}%":"unavailable";
            GetNode<Label>("Scroll/Content/RuntimeStatus").Text+=$"\nCPU {Percent(m.Cpu)} | RAM {Percent(m.Ram)}\nOutput volume {Percent(m.Volume)}{(m.Muted?" (muted)":"")} | {(s.Hot?"Heat response":"Normal temperature")}\nHotkey: {(Systems.Hotkey.Registered?"Ctrl+Shift+L":"unavailable — use button / tray")}";
            _resetButton!.Disabled=s.Locked;
        }
    }

    public void RequestExit()
    {
        if (_exiting) return;
        _exiting = true;
        GD.Print("SITE_DESKTOP_EXIT_REQUESTED");
        Systems?.Shutdown();
        Runtime.Shutdown();Scps.Shutdown();Objects?.Shutdown();Icons?.Shutdown();
        GetTree().Quit();
    }

    public override void _ExitTree()
    {
        if (_exitButton is not null)
            _exitButton.Pressed -= ControlButtonPressed;
        if (_visibilityButton is not null) _visibilityButton.Pressed -= ToggleOverlays;
        if (_resetButton is not null) _resetButton.Pressed -= ResetOverlays;
        if(_spawnButton!=null)_spawnButton.Pressed-=SpawnData;
        if(_strongButton!=null)_strongButton.Toggled-=StrongChanged;
        Systems?.Shutdown();
        if(Scps!=null){Scps.Changed-=ActivationChanged;Runtime.Shutdown();Scps.Shutdown();}
        Objects?.Shutdown();
        Icons?.Shutdown();
        GetWindow().CloseRequested -= HideControls;
        GD.Print("SITE_DESKTOP_STOPPED");
    }
}
