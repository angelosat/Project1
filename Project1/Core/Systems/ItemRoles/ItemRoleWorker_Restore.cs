using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
using System;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleWorker_Restore(ResourceDef resource) : ItemRoleWorker
{
    public override int GetInventoryScore(Actor actor, Entity item)
    {
        if (!item.TryGetComponent<ConsumableComp>(out var comp))
            return -1;
        var magnitude = comp.GetMagnitude(EffectDefOf.RestoreResource, resource);
        return magnitude;
    }

    public override int GetSituationalScore(Actor actor, Entity item)
    {
        var comp = item.GetComponent<ConsumableComp>();
        var magnitude = comp.GetMagnitude(EffectDefOf.RestoreResource, resource);
        var deficit = actor.Resources.GetDeficit(resource);
        return Math.Min(deficit, magnitude);
        //return deficit > magnitude ? deficit : -1;
    }
}
