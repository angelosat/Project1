using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleContextWorker_Nutrition : ItemRoleContextWorker
{
    internal override IEnumerable<ItemRoleDef> GenerateRoles(ItemRoleContextDef contextDef)
        => GenerateAtomic(contextDef, typeof(ItemRoleWorker_Nutrition));
}
