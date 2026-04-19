using Project1.Core.Entities;
using Project1.Core.Systems.Alchemy;
using System;
using System.Linq;

namespace Project1.Core.Systems.Consumables;

internal sealed class ConsumableWorker_Food : ConsumableWorker
{
    internal override string GetLabel(ConsumableComp comp)
    {
        throw new NotImplementedException();
    }
}
internal sealed class ConsumableWorker_Potion : ConsumableWorker 
{
    internal override void PostProcess(Entity entity)
    {
        PotionSystem.PostProcess(entity);
    }
    internal override string GetLabel(ConsumableComp comp)
    {
        //var effect = comp.EffectsNew.First();
        return PotionSystem.GetName(comp);
        //return $"{ConsumableDefOf.Potion.LabelReadable} of {effect.Def.Verb} {effect.Target.LabelReadable}";
    }
}
internal sealed class ConsumableWorker_Scroll : ConsumableWorker 
{
    internal override string GetLabel(ConsumableComp comp)
    {
        var effect = comp.EffectsNew.First();
        return $"{ConsumableDefOf.Scroll.LabelReadable} of {effect.Def.Verb} {effect.Target.LabelReadable}";
    }
}
public abstract class ConsumableWorker
{
    internal abstract string GetLabel(ConsumableComp comp);
    internal virtual void PostProcess(Entity entity) { }
}
