using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Consumables;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleContextWorker_TownScroll : ItemRoleContextWorker
{
    internal override IEnumerable<ItemRoleDef> GenerateRoles(ItemRoleContextDef contextDef)
        => this.GenerateAtomic(contextDef, typeof(ItemRoleWorker_TownScroll));
}

sealed class ItemRoleWorker_TownScroll : ItemRoleWorker
{
    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context)
    {
        if (item.Def != ItemDefOf.Consumable)
            return 0;
        if (item.Profile != ConsumableDefOf.Scroll)
            return 0;
        return 100;
    }

    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
    {
        return -1;
    }

    public override IEnumerable<Def> GetValidTargetDefs()
        => [null];
}