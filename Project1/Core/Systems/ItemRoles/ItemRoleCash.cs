using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Consumables;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleCash : ItemRoleWorker
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

sealed class ItemRoleTownScroll : ItemRoleWorker
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

    //public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
    //    => 0;
    //public override IEnumerable<ItemRoleKey> GenerateKeys()
    //{
    //    yield return new ItemRoleKey_TownScroll();
    //}

    //public override int GetInventoryScore(Actor actor, Entity item, ItemRoleKey key)
    //{
    //    throw new System.NotImplementedException();
    //}

    //public override int GetSituationalScore(Actor actor, Entity item, ItemRoleKey key)
    //{
    //    throw new System.NotImplementedException();
    //}
}