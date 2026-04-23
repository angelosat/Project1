using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRole_Restore : ItemRoleWorker
{
    public override IEnumerable<Def> GetValidTargetDefs()
       => Def.Get<ResourceDef>().Where(r => r.SupportsRestore);

    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context)
    {
        if (!item.TryGetComponent<ConsumableComp>(out var comp))
            return -1;
        var resource = (ResourceDef)context.Def;
        if (comp.Effect is not EntityEffectWrapper effect)
            return -1;
        if (effect.Def != EffectDefOf.RestoreResource)
            return -1;
        if (effect.Target != resource)
            return -1;
        return (int)effect.MagnitudeFinal;
    }

    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
    {
        var def = (ResourceDef)context.Def;
        var comp = item.GetComponent<ConsumableComp>();
        var effect = comp.Effect;
        if (!(effect.Def == EffectDefOf.RestoreResource && effect.Target == def))
            throw new System.Exception();

        var deficit = actor.Resources.GetDeficit(def);
        //return deficit;
        return deficit > effect.MagnitudeFinal ? deficit : -1;
    }
}
