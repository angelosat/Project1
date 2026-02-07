using Project1.Core.Attributes;
using Project1.Core.Entities;

namespace Project1.Core.Entities.Stats.ValueGetters
{
    class StatMaxHaulWeight : StatWorker
    {
        public override float CalculateStat(GameObject obj)
        {
            return obj[AttributeDefOf.Strength]?.Level ?? 0;
        }
    }
}
