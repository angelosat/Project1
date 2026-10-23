using Project1.Core.Entities;
using Project1.Core.Entities.Stats;

namespace Project1.Core.Attributes;

public abstract class AttributeWorker
{
    public abstract void Tick(Entity obj, AttributeRuntime attributeStat);
    internal virtual void Award(Entity obj, AttributeRuntime attributeStat, float p)
    {
        attributeStat.AddToProgress(p);
    }

    internal virtual float GetStatMod(StatDef stat, int level) => 0f;
}
