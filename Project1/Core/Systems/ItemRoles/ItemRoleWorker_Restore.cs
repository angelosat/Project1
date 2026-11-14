using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleWorker_Restore(ResourceDef resource) : ItemRoleWorker
{
    public override int GetInventoryScore(Actor actor, Entity item)
    {
        if (!item.TryGetComponent<ConsumableComp>(out var comp))
            return -1;
        if (comp.Effect is not EntityEffectWrapper effect)
            return -1;
        if (effect.Def != EffectDefOf.RestoreResource)
            return -1;
        if (effect.Target != resource)
            return -1;
        return (int)effect.MagnitudeFinal;
    }

    public override int GetSituationalScore(Actor actor, Entity item)
    {
        var comp = item.GetComponent<ConsumableComp>();
        var effect = comp.Effect;
        if (!(effect.Def == EffectDefOf.RestoreResource && effect.Target == resource))
            throw new System.Exception();

        var deficit = actor.Resources.GetDeficit(resource);
        return deficit > effect.MagnitudeFinal ? deficit : -1;
    }
}
