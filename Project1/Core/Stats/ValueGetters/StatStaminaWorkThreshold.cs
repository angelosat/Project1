using Project1.Core.AI;
using Project1.Core.Entities;
using Project1.Core.Resources;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Entities.Stats.ValueGetters
{
    class StatStaminaWorkThreshold : StatWorker
    {
        public override float CalculateStat(GameObject obj)
        {
            var actor = obj as Actor;
            var staminaBaseThreshold = .25f; //placeholder?
            var stamina = actor.GetResource(ResourceDefOf.Stamina);
            staminaBaseThreshold = stamina.GetThresholdValue(0);
            var activity1 = actor.GetTrait(TraitDefOf.Activity).Normalized;
            var num = activity1 * staminaBaseThreshold * .5f;
            var threshold = staminaBaseThreshold - num;
            return threshold;
        }
    }
}
