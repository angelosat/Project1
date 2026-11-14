using Project1.Core.Systems.Tools;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

internal sealed class ItemRoleContextWorker_Tool : ItemRoleContextWorker
{
    internal override IEnumerable<ItemRoleDef> GenerateRoles(ItemRoleContextDef contextDef)
    {
        var defs = Def.Get<ToolUseDef>();
        foreach (var def in defs)
            yield return new ItemRoleDef($"{contextDef.LabelReadable}:{def.LabelReadable}", contextDef)
            {
                Worker = new ItemRoleWorker_Tool(def)
            };
    }
}
