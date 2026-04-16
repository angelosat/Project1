using Microsoft.Xna.Framework;
using Project1.Core.Stats;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatEncumberance : StatResolver
{
    public override float CalculateStat(Entity obj)
    {
        var haulWeight = obj.Hauled?.TotalWeight ?? 0;
        if (haulWeight == 0)
            return 0;
        var maxWeight = StatDefOf.MaxHaulWeight.CalculateFor(obj);
        var ratio = haulWeight / maxWeight;
        ratio = MathHelper.Clamp(ratio, 0, 1);
        return ratio;
    }
}
