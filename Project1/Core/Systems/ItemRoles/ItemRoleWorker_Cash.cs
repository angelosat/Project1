using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;
sealed class ItemRoleWorker_Cash : ItemRoleWorker
{
    public override int GetInventoryScore(Actor actor, Entity item)
    {
        if (actor.IsTownMember)
            return 0;
        if (item.Def != ItemDefOf.Coins)
            return 0;
        return 100;
    }

    public override int GetSituationalScore(Actor actor, Entity item)
        => 0;
}
