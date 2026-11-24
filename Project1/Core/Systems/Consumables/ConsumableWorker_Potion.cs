using Project1.Core.Entities;
using Project1.Core.Systems.Alchemy;
using System;

namespace Project1.Core.Systems.Consumables;

internal sealed class ConsumableWorker_Potion : ConsumableWorker
{
    internal override void PostProcess(Entity entity)
        => PotionSystem.PostProcess(entity);

    internal override string GetLabel(ConsumableComp comp)
    {
        throw new NotImplementedException();
        //var effect = comp.EffectsNew.First();
        //return $"{ConsumableDefOf.Potion.LabelReadable} of {effect.Def.Verb} {effect.Target.LabelReadable}";
    }
}
