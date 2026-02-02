using Project1.Framework.Attributes;
using Start_a_Town_;

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
