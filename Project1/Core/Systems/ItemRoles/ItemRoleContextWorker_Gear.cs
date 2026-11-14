using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

internal sealed class ItemRoleContextWorker_Gear : ItemRoleContextWorker
{
    internal override IEnumerable<ItemRoleDef> GenerateRoles(ItemRoleContextDef contextDef)
    {
        var gearSlotDefs = Def.Get<GearSlotDef>();
        var statDefs = Def.Get<StatDef>();
        foreach (var slotDef in gearSlotDefs)
            foreach (var statDef in statDefs)
                yield return GenerateAdvanced(contextDef, typeof(ItemRoleWorker_Gear), slotDef, statDef);
    }
}
