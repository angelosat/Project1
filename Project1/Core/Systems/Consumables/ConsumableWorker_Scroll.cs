using Project1.Core.Entities;

namespace Project1.Core.Systems.Consumables;

internal sealed class ConsumableWorker_Scroll : ConsumableWorker
{
    internal override string GetLabel(ConsumableComp comp)
    {
        var effect = comp.Spell;
        return $"{ConsumableDefOf.Scroll.LabelReadable} of {effect.LabelReadable}";
    }
    internal override void PostProcess(Entity entity)
    {
        entity.Name = GetLabel(entity.Consumable);
    }
}
