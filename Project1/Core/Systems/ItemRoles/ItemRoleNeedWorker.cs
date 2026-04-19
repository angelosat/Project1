using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleNeedWorker : ItemRoleWorker
{
    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef role)
    {
        var needDef = (NeedDef)role.Def;
        if (actor.Needs.GetPercentage(needDef) > .9f)
            return -100;
        if (!item.TryGetComponent<ConsumableComp>(out var consumableComp))
            return -100;
        var needRestore = consumableComp.EffectsNew.Where(e => e.Target == needDef).Sum(e => e.Budget);
        if (needRestore <= 0)
            return -100;
        var need = actor.GetNeed(needDef);
        var needDeficit = need.Max - need.Value;// need.Deficit;
        return (int)(needRestore * needDeficit);
    }
    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef role)
    {
        if (!item.TryGetComponent<ConsumableComp>(out var consumableComp))
            return -1;
        var benefit = consumableComp.EffectsNew.Where(e => e.Target == role.Def).Sum(e => e.Budget);
        return (int)(benefit.HasValue ? benefit.Value * item.StackMax : 0);
    }

    public override IEnumerable<Def> GetValidTargetDefs()
        => Def.Get<NeedDef>();
    //public override IEnumerable<ItemRoleKey> GenerateKeys()
    //{
    //    return Def.Get<NeedDef>().Select(n=> new ItemRoleKey_Need(n));
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