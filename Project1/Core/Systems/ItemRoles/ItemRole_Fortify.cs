using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRole_Fortify : ItemRoleWorker
{
    public override IEnumerable<Def> GetValidTargetDefs()
        => Def.Get<ResourceDef>().Where(r => r.SupportsFortify);

    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context)
    {
        if (!item.TryGetComponent<ConsumableComp>(out var comp))
            return -1;
        var resource = (ResourceDef)context.Def;
        if (comp.Effect is not EntityEffectWrapper effect)
            return -1;
        if (effect.Def != EffectDefOf.FortifyResource)
            return -1;
        if (effect.Target != resource)
            return -1;
        return (int)effect.MagnitudeFinal;
    }

    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
    {
        var def = context.Def;
        if (actor.Effects.Any(EffectDefOf.FortifyResource, def))
            return 0;
        var comp = item.GetComponent<ConsumableComp>();
        if (!(comp.Effect.Def == EffectDefOf.FortifyResource && comp.Effect.Target == def))
            throw new System.Exception();
        return (int)comp.Effect.MagnitudeFinal;
    }

}
