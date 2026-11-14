using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleContextWorker_Restore : ItemRoleContextWorker
{
    internal override IEnumerable<ItemRoleDef> GenerateRoles(ItemRoleContextDef contextDef)
    //=> this.GenerateRolesSimple<ResourceDef>(r => r.SupportsRestore, contextDef, typeof(ItemRoleWorker_Restore));
    {
        foreach (var def in Def.Get<ResourceDef>().Where(r => r.SupportsRestore))
            yield return GenerateAdvanced(contextDef, typeof(ItemRoleWorker_Restore), def);
    }
}

sealed class ItemRoleWorker_Restore(ResourceDef resource) : ItemRoleWorker
{
    public override IEnumerable<Def> GetValidTargetDefs()
       => Def.Get<ResourceDef>().Where(r => r.SupportsRestore);

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
        //var def = (ResourceDef)context.Source;
        var comp = item.GetComponent<ConsumableComp>();
        var effect = comp.Effect;
        if (!(effect.Def == EffectDefOf.RestoreResource && effect.Target == resource))
            throw new System.Exception();

        var deficit = actor.Resources.GetDeficit(resource);
        //return deficit;
        return deficit > effect.MagnitudeFinal ? deficit : -1;
    }
}
