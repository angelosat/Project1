using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

[EnsureStaticCtorCall]
public class ItemRoleDefOf
{
    public static readonly ItemRoleDef Nutrition = new(ItemRoleContextDefOf.Nutrition, null) { Worker = new ItemRoleWorker_Nutrition() };

    static ItemRoleDefOf()
    {
        var contexts = Def.Get<ItemRoleContextDef>();

        var toRegister = new List<ItemRoleDef>();
        //foreach (var contextDef in contexts)
        //    toRegister.AddRange(contextDef.WorkerRole.GetValidTargetDefs().Select(d => new ItemRoleDef(contextDef, d)));
        foreach (var contextDef in contexts)
            toRegister.AddRange(contextDef.WorkerContext.GenerateRoles(contextDef));
        foreach (var role in toRegister)
            Def.Register(role);
    }
}
