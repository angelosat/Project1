using Project1.Core.Entities;
using Project1.Core.Systems.Alchemy;
using System;

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
        => PotionSystem.PostProcess(entity);
    
    internal override string GetLabel(ConsumableComp comp)
        => PotionSystem.GetName(comp);
    
}
internal sealed class ConsumableWorker_Scroll : ConsumableWorker 
{
    internal override string GetLabel(ConsumableComp comp)
    {
        var effect = comp.Spell;
        return $"{ConsumableDefOf.Scroll.LabelReadable} of {effect.LabelReadable}";
    }
    internal override void PostProcess(Entity entity)
    {
        entity.Name = this.GetLabel(entity.Consumable);
    }
}
public abstract class ConsumableWorker
{
    internal abstract string GetLabel(ConsumableComp comp);
    internal virtual void PostProcess(Entity entity) { }
}
