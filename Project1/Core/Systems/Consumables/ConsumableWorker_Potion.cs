using Project1.Core.Entities;
using Project1.Core.Systems.Alchemy;

namespace Project1.Core.Systems.Consumables;

internal sealed class ConsumableWorker_Potion : ConsumableWorker
{
    internal override void PostProcess(Entity entity)
        => PotionSystem.PostProcess(entity);

    internal override string GetLabel(ConsumableComp comp)
        => PotionSystem.GetName(comp);
}
