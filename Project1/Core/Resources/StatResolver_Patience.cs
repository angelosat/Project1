using Project1.Core.AI;
using Project1.Core.AI.Personality;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats.Resolvers;

namespace Project1.Core.Resources;

internal class StatResolver_Patience : StatResolver
{
    public override float CalculateStat(Entity obj)
    {
        var actor = obj as Actor;
        var patience = actor.Personality.GetValue(TraitDefOf.Patience);
        return 100 + patience / 2f;
    }
}
