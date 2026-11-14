using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleWorker_Fortify(ResourceDef resource) : ItemRoleWorker
{
    public override int GetInventoryScore(Actor actor, Entity item)
    {
        if (!item.TryGetComponent<ConsumableComp>(out var comp))
            return -1;
        if (comp.Effect is not EntityEffectWrapper effect)
            return -1;
        if (effect.Def != EffectDefOf.FortifyResource)
            return -1;
        if (effect.Target != resource)
            return -1;
        return (int)effect.MagnitudeFinal;
    }

    public override int GetSituationalScore(Actor actor, Entity item)
    {
        if (actor.Effects.Any(EffectDefOf.FortifyResource, resource))
            return 0;
        var comp = item.GetComponent<ConsumableComp>();
        if (!(comp.Effect.Def == EffectDefOf.FortifyResource && comp.Effect.Target == resource))
            throw new System.Exception();
        return (int)comp.Effect.MagnitudeFinal;
    }
}
