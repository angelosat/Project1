using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;
sealed class ItemRoleGearWorker : ItemRoleWorker
{
    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
    {
        throw new System.NotImplementedException();
    }
    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context)
    {
        var props = item.Def.ApparelProperties;
        if (props?.GearType != context.Def)
            return -1;
        return props.ArmorValue;
    }

    public override IEnumerable<Def> GetValidTargetDefs()
        => Def.Get<GearTypeDef>();
    //public override IEnumerable<ItemRoleKey> GenerateKeys()
    //{
    //    yield return new ItemRoleKey_Gear();
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
