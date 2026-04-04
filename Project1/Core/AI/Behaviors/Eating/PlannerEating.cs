using Microsoft.Xna.Framework;
using Project1.Core.AI.Reservations;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Core.Towns;
using Project1.Framework;
using System.Linq;

namespace Project1.Core.AI.Behaviors.Eating
{
    class PlannerEating : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (actor.Needs.GetPercentage(NeedDefOf.Hunger) > .9f)
                return null;

            // if food in hands, eat it
            if (actor.Hauled is not Entity carried)
                return null;

            //// if carrying non-consumable, exit
            //if (!carried.TryGetComponent<ConsumableComponent>(out var comp))
            //    return null;

            //// if carrying non-food, exit
            //if (!comp.HasEffectTarget(NeedDefOf.Hunger))
            //    return null;

            if (HungerUtility.GetNutrition(actor, carried) <= 0)
                return null;

            return new Plan(PlanDefOf.Eating, carried);
        }
    }
}
