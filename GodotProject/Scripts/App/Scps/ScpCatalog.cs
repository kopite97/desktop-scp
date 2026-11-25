using System;
using System.Collections.Generic;
using SiteDesktop.Overlay;

namespace SiteDesktop.App.Scps;

public sealed record ScpDefinition(string Id,string Name,string Description,bool DefaultEnabled,
    Func<bool,IScpModule> Create,bool UsesObjects=false,bool OwnsZombies=false,string? RuntimeOwner=null);

public static class ScpCatalog
{
    public static IReadOnlyList<ScpDefinition> Create(ScpCreationContext context,bool enablePlague=true,bool enableStatue=true)
    {
        var entries=new List<ScpDefinition>();
        entries.Add(new("scp-999","SCP-999","커서 따라가기 · 쓰다듬기",true,active=>
        {
            var controller=new Scp999Controller(){Environment=context.Environment,Stress=context.Stress,Stack=context.Stack,InitialActive=active};context.Register("scp-999",controller,controller.RuntimeTick);
            return new Scp999Module(controller);
        }));
        if(enablePlague)entries.Add(new("scp-049","SCP-049","아이콘 치료 · 좀비와 함께 켜고 끄기",true,active=>
        {
            var controller=new PlagueController(){Environment=context.Environment,Stress=context.Stress,Stack=context.Stack,Objects=context.Objects!,InitialActive=active};context.Register("scp-049",controller,controller.RuntimeTick);
            return new Scp049Module(controller);
        },true,true,"scp049"));
        if(enableStatue)entries.Add(new("scp-173","SCP-173","시선 반응 · 아이콘 파손",true,active=>
        {
            var controller=new StatueController(){Environment=context.Environment,SharedStress=context.Stress,Stack=context.Stack,Objects=context.Objects!,InitialActive=active};context.Register("scp-173",controller,controller.RuntimeTick);
            return new Scp173Module(controller);
        },true,false,"scp173"));
        return entries;
    }
}
