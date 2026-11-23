using Project1.Core.Entities;

namespace Project1.Core.Systems.Consumables;
public abstract class ConsumableWorker
{
    internal abstract string GetLabel(ConsumableComp comp);
    internal virtual void PostProcess(Entity entity) { }
}
