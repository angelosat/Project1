using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

//sealed class ItemRoleWorker_GearOther(GearSlotDef slot) : ItemRoleWorker
//{
//    public override int GetSituationalScore(Actor actor, Entity item)
//    {
//        throw new System.NotImplementedException();
//    }
//    public override int GetInventoryScore(Actor actor, Entity item)
//    {
//        var props = item.Def.ApparelProperties;
//        if (props?.GearType != context.Source)
//            return -1;
//        return props.ArmorValue;
//    }

//    public override IEnumerable<Def> GetValidTargetDefs()
//        => Def.Get<GearSlotDef>();
//    //public override IEnumerable<ItemRoleKey> GenerateKeys()
//    //{
//    //    yield return new ItemRoleKey_Gear();
//    //}

//    //public override int GetInventoryScore(Actor actor, Entity item, ItemRoleKey key)
//    //{
//    //    throw new System.NotImplementedException();
//    //}

//    //public override int GetSituationalScore(Actor actor, Entity item, ItemRoleKey key)
//    //{
//    //    throw new System.NotImplementedException();
//    //}
//}
