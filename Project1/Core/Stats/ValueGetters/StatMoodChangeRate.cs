using Project1.Core.AI;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Entities.Stats.ValueGetters
{
    class StatMoodChangeRate : StatWorker
    {
        public override float CalculateStat(GameObject obj)
        {
            var actor = obj as Actor;
            var resilience = actor.GetTrait(TraitDefOf.Resilience).Normalized;
            var value = 1 + resilience * .5f;
            return value;
        }
    }
}
