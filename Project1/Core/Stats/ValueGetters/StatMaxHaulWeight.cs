using Project1.Core.Attributes;

namespace Project1.Core.Entities.Stats.ValueGetters
{
    sealed class StatMaxHaulWeight : StatWorker
    {
        public override float CalculateStat(Entity obj)
        {
            return obj[AttributeDefOf.Strength]?.Level ?? 0;
        }
    }
}
