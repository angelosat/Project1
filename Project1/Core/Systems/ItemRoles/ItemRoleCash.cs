using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Consumables;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleFortify : ItemRoleWorker
{
    public override IEnumerable<Def> GetValidTargetDefs()
        => Def.Get<ResourceDef>().Where(r => r.SupportsFortify);

    //public override IEnumerable<ItemRoleKey> GenerateKeys()
    //{
    //    throw new System.NotImplementedException();
    //}

    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context)
    {
        throw new System.NotImplementedException();
    }

    //public override int GetInventoryScore(Actor actor, Entity item, ItemRoleKey key)
    //{
    //    throw new System.NotImplementedException();
    //}

    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
    {
        throw new System.NotImplementedException();
    }

    //public override int GetSituationalScore(Actor actor, Entity item, ItemRoleKey key)
    //{
    //    throw new System.NotImplementedException();
    //}
}
sealed class ItemRoleRestore : ItemRoleWorker
{
    public override IEnumerable<Def> GetValidTargetDefs()
       => Def.Get<ResourceDef>().Where(r => r.SupportsRestore);

    //public override IEnumerable<ItemRoleKey> GenerateKeys()
    //{
    //    throw new System.NotImplementedException();
    //}

    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context)
    {
        throw new System.NotImplementedException();
    }

    //public override int GetInventoryScore(Actor actor, Entity item, ItemRoleKey key)
    //{
    //    throw new System.NotImplementedException();
    //}

    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
    {
        throw new System.NotImplementedException();
    }

    //public override int GetSituationalScore(Actor actor, Entity item, ItemRoleKey key)
    //{
    //    throw new System.NotImplementedException();
    //}
}

//sealed class ItemRolePotion : ItemRoleWorker
//{
//    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context)
//    {
//        throw new System.NotImplementedException();
//    }

//    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
//    {
//        throw new System.NotImplementedException();
//    }

//    public override IEnumerable<ItemRoleKey> GenerateKeys()
//        => PotionSystem.Recipes.Select(a => new ItemRoleKey_Potion(a.effect, a.target));

//    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleKey key)
//    {
//        if (item.Def != ItemDefOf.Consumable)
//            return -1;
//        if (item.Profile != ConsumableDefOf.Potion)
//            return -1;
//        var typedKey = (ItemRoleKey_Potion)key;
//        var comp = item.GetComponent<ConsumableComp>();
//        var effect = comp.Effect;
//        if (!(effect.Def == typedKey.Effect && effect.Target == typedKey.Target))
//            return -1;
//        return (int)(effect.Magnitude * comp.Tier);
//    }

//    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleKey key)
//    {
//        throw new System.NotImplementedException();
//    }
//}
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

    //public override IEnumerable<ItemRoleKey> GenerateKeys()
    //{
    //    yield return new ItemRoleKey_Cash();
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
        throw new System.NotImplementedException();
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