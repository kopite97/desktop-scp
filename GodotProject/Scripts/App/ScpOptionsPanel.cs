using Godot;
using SiteDesktop.App.Scps;
using System.Collections.Generic;

namespace SiteDesktop.App;

public partial class ScpOptionsPanel : VBoxContainer
{
    public ScpActivationManager Manager { get; init; }=null!;
    public ScpSettingsStore Store { get; init; }=null!;
    public Dictionary<string,CheckButton> Toggles { get; }=new();
    private readonly Dictionary<string,Label> _states=new();
    private Label _save=null!;
    public override void _Ready()
    {
        AddThemeConstantOverride("separation",4);AddChild(new Label{Text="SCP 활성화",ThemeTypeVariation="HeaderSmall"});
        foreach(var entry in Manager.Entries)
        {
            string id=entry.Definition.Id;var row=new HBoxContainer();AddChild(row);
            var toggle=new CheckButton{Text=entry.Definition.Name,TooltipText=entry.Definition.Description,CustomMinimumSize=new(170,36)};
            row.AddChild(toggle);Toggles.Add(id,toggle);toggle.Toggled+=value=>Manager.SetEnabled(id,value);
            var state=new Label{SizeFlagsHorizontal=SizeFlags.ExpandFill};row.AddChild(state);_states.Add(id,state);
        }
        _save=new Label{AutowrapMode=TextServer.AutowrapMode.WordSmart};AddChild(_save);
        Manager.Changed+=Refresh;Refresh(null);
    }
    public void Refresh(string? id)
    {
        foreach(var entry in Manager.Entries){Toggles[entry.Definition.Id].SetPressedNoSignal(entry.Enabled);_states[entry.Definition.Id].Text=entry.State;}
        _save.Text=Store.Status;
    }
    public override void _ExitTree(){Manager.Changed-=Refresh;}
}
