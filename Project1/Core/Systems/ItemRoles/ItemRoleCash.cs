using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleFortify : ItemRoleWorker
{
    public override IEnumerable<Def> GetValidTargetDefs()
        => Def.Get<ResourceDef>().Where(r => r.SupportsFortify);

    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context)
    {
        if (!item.TryGetComponent<ConsumableComp>(out var comp))
            return -1;
        var resource = (ResourceDef)context.Def;
        if (comp.Effect.Def != EffectDefOf.FortifyResource)
            return -1;
        if (comp.Effect.Target != resource)
            return -1;
        return (int)comp.Effect.Magnitude;
    }

    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
    {
        var def = context.Def;
        if (actor.Effects.Any(EffectDefOf.FortifyResource, def))
            return 0;
        var comp = item.GetComponent<ConsumableComp>();
        if (!(comp.Effect.Def == EffectDefOf.FortifyResource && comp.Effect.Target == def))
            throw new System.Exception();
        return (int)comp.Effect.Magnitude;
    }

}
sealed class ItemRoleRestore : ItemRoleWorker
{
    public override IEnumerable<Def> GetValidTargetDefs()
       => Def.Get<ResourceDef>().Where(r => r.SupportsRestore);

    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context)
    {
        if (!item.TryGetComponent<ConsumableComp>(out var comp))
            return -1;
        var resource = (ResourceDef)context.Def;
        if (comp.Effect.Def != EffectDefOf.RestoreResource)
            return -1;
        if (comp.Effect.Target != resource)
            return -1;
        return (int)comp.Effect.Magnitude;
    }

    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
    {
        var def = (ResourceDef)context.Def;
        var comp = item.GetComponent<ConsumableComp>();
        if (!(comp.Effect.Def == EffectDefOf.RestoreResource && comp.Effect.Target == def))
            throw new System.Exception();
        return (int)actor.Resources.GetDeficit(def);
    }
}

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