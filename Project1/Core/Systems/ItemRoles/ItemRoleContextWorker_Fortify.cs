using Project1.Core.Resources;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleContextWorker_Fortify : ItemRoleContextWorker
{
    internal override IEnumerable<ItemRoleDef> GenerateRoles(ItemRoleContextDef contextDef)
    {
        foreach (var def in Def.Get<ResourceDef>().Where(r => r.SupportsFortify))
            yield return GenerateAdvanced(contextDef, typeof(ItemRoleWorker_Fortify), def);
    }
}
