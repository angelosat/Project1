using Project1.Core.AI.Personality;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatMoodChangeRate : StatResolver
{
    public override float CalculateStat(Entity obj)
    {
        var actor = obj as Actor;
        var resilience = actor.GetTrait(TraitDefOf.Resilience).Normalized;
        var value = 1 + resilience * .5f;
        return value;
    }
}
