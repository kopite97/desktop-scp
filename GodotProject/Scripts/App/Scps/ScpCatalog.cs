using System;
using System.Collections.Generic;
using SiteDesktop.Overlay;

namespace SiteDesktop.App.Scps;

public sealed record ScpDefinition(string Id,string Name,string Description,bool DefaultEnabled,
    Func<bool,IScpModule> Create,bool UsesObjects=false,bool OwnsZombies=false,string? RuntimeOwner=null);

public static class ScpCatalog
{
    public static IReadOnlyList<ScpDefinition> Create(Main app)
    {
        var entries=new List<ScpDefinition>();
        entries.Add(new("scp-999","SCP-999","커서 따라가기 · 쓰다듬기",true,active=>
        {
            app.Companion=new(){Stress=app.Stress,Stack=app.Stack,InitialActive=active};app.AddChild(app.Companion);
            return new Scp999Module(app.Companion);
        }));
        if(app.EnablePlague)entries.Add(new("scp-049","SCP-049","아이콘 치료 · 좀비와 함께 켜고 끄기",true,active=>
        {
            app.Plague=new(){Stress=app.Stress,Stack=app.Stack,Objects=app.Objects!,InitialActive=active};app.AddChild(app.Plague);
            return new Scp049Module(app.Plague);
        },true,true,"scp049"));
        if(app.EnableStatue)entries.Add(new("scp-173","SCP-173","시선 반응 · 아이콘 파손",true,active=>
        {
            app.Statue=new(){SharedStress=app.Stress,Stack=app.Stack,Objects=app.Objects!,InitialActive=active};app.AddChild(app.Statue);
            return new Scp173Module(app.Statue);
        },true,false,"scp173"));
        return entries;
    }
}
