using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleWorker_Need(NeedDef needDef) : ItemRoleWorker
{
    public override int GetSituationalScore(Actor actor, Entity item)
    {
        if (actor.Needs.GetPercentage(needDef) > .9f)
            return -100;
        if (!item.TryGetComponent<ConsumableComp>(out var comp))
            return -100;
        //var needRestore = comp.EffectsNew.Where(e => e.Target == needDef).Sum(e => e.Budget);
        var needRestore = comp.GetMagnitude(EffectDefOf.ModifyNeed, needDef);
        if (needRestore <= 0)
            return -100;
        var need = actor.GetNeed(needDef);
        var needDeficit = need.Max - need.Value;// need.Deficit;
        return (int)(needRestore * needDeficit);
    }

    public override int GetInventoryScore(Actor actor, Entity item)
    {
        if (!item.TryGetComponent<ConsumableComp>(out var comp))
            return -1;
        //var benefit = consumableComp.EffectsNew.Where(e => e.Target == needDef).Sum(e => e.Budget);
        //return (int)(benefit.HasValue ? benefit.Value * item.StackMax : 0);

        var benefit = comp.GetMagnitude(EffectDefOf.ModifyNeed, needDef);
        return benefit * item.StackMax;
    }
}
