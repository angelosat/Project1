using Microsoft.Xna.Framework;

namespace Start_a_Town_
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
