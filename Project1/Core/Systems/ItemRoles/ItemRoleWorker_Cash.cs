using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleContextWorker_Cash : ItemRoleContextWorker
{
    internal override IEnumerable<ItemRoleDef> GenerateRoles(ItemRoleContextDef contextDef)
        => this.GenerateAtomic(contextDef, typeof(ItemRoleWorker_Cash));
}
sealed class ItemRoleWorker_Cash : ItemRoleWorker
{
    public override IEnumerable<Def> GetValidTargetDefs()
        => [null];

    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context)
    {
        if (actor.IsTownMember)
            return 0;
        if (item.Def != ItemDefOf.Coins)
            return 0;
        return 100;
    }

    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
        => 0;
}
