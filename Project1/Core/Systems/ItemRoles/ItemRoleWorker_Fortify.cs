using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleWorker_Fortify(ResourceDef resource) : ItemRoleWorker
{
    public override int GetInventoryScore(Actor actor, Entity item)
    {
        if (!item.TryGetComponent<ConsumableComp>(out var comp))
            return -1;
        var magnitude = comp.GetMagnitude(EffectDefOf.ResourceFortify, resource);
        return magnitude;
    }

    public override int GetSituationalScore(Actor actor, Entity item)
    {
        if (actor.Effects.Any(EffectDefOf.ResourceFortify, resource))
            return 0;
        var comp = item.GetComponent<ConsumableComp>();
        var magnitude = comp.GetMagnitude(EffectDefOf.ResourceFortify, resource);
        return magnitude;
    }
}
