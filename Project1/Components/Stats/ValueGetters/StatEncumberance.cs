using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class StatEncumberance : StatValueGetter
    {
        public StatEncumberance(StatDef parent) : base(parent)
        {
        }

        public override float GetValue(GameObject obj)
        {
            var haulWeight = obj.Hauled?.TotalWeight ?? 0;
            if (haulWeight == 0)
                return 0;
            var maxWeight = StatDefOf.MaxHaulWeight.GetValue(obj);
            var ratio = haulWeight / maxWeight;
            ratio = MathHelper.Clamp(ratio, 0, 1);
            return ratio;
        }
    }
}
