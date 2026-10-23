using Project1.Core.Entities;
using Project1.Core.Entities.Stats;
using Project1.Core.Stats;

namespace Project1.Core.Attributes;

sealed class AttributeDexterity : AttributeWorker
{
    public override void Tick(Entity obj, AttributeRuntime attributeStat)
    {
    }

    internal override float GetStatMod(StatDef stat, int level)
    {
        if (stat == StatDefOf.WalkSpeed ||
            stat == StatDefOf.HitChance)
            return level;
        return 0f;
    }
}
