using Project1.Core.Entities;
using Project1.Core.Resources;
using Project1.Core.Entities.Actors;
using Project1.Core.AI.Personality;

namespace Project1.Core.Entities.Stats.ValueGetters
{
    sealed class StatStaminaWorkThreshold : StatWorker
    {
        public override float CalculateStat(Entity obj)
        {
            var actor = obj as Actor;
            var staminaBaseThreshold = .25f; //placeholder?
            var stamina = actor.GetResource(ResourceDefOf.Stamina);
            staminaBaseThreshold = stamina.GetThresholdValue(0);
            var activity1 = actor.GetTrait(TraitDefOf.Drive).Normalized;
            var num = activity1 * staminaBaseThreshold * .5f;
            var threshold = staminaBaseThreshold - num;
            return threshold;
        }
    }
}
