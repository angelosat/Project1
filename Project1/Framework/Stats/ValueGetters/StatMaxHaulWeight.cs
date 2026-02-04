using Project1.Framework.Attributes;
using Project1.Framework.Entities;

namespace Project1.Framework.Stats.ValueGetters
{
    class StatMaxHaulWeight : StatWorker
    {
        public override float CalculateStat(GameObject obj)
        {
            return obj[AttributeDefOf.Strength]?.Level ?? 0;
        }
    }
}
