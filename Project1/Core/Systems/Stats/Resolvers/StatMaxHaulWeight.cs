using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatMaxHaulWeight : StatResolver
{
    public override float CalculateStat(Entity obj)
    {
        return obj[AttributeDefOf.Strength]?.Level ?? 0;
    }
}
