using Project1.Core.Needs;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleContextWorker_Need : ItemRoleContextWorker
{
    internal override IEnumerable<ItemRoleDef> GenerateRoles(ItemRoleContextDef contextDef)
    {
        foreach (var def in Def.Get<NeedDef>())
            yield return GenerateAdvanced(contextDef, typeof(ItemRoleWorker_Need), def);
    }
}
