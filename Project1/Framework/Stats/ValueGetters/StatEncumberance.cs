using Microsoft.Xna.Framework;
using Project1.Framework.Stats;
using Start_a_Town_;

namespace Project1.Framework.Stats.ValueGetters
{
    class StatEncumberance : StatWorker
    {
        public override float CalculateStat(GameObject obj)
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
}
