using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Core.Entities;
using Project1.Core.Needs;
using Project1.Core.Towns;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;

namespace Project1.Core.AI.Behaviors.Eating
{
    class EatingPlanner : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            // if food in hands, eat it
            if (actor.Hauled is not Entity carried)
                return null;

            // if carrying non-consumable, exit
            if (!carried.TryGetComponent<ConsumableComponent>(out var comp))
                return null;

            // if carrying non-food, exit
            if (!comp.HasEffectTarget(NeedDefOf.Hunger))
                return null;

            return new Plan(PlanDefOf.Eating, carried);
        }
        
        private static IOrderedEnumerable<IntVec3> FindEatingPlaces(Actor actor)
        {
            return actor.Map.Town.GetUtilities(Utility.Types.Eating).Where(p => actor.CanReserve(p)).OrderBy(p => Vector3.DistanceSquared(p, actor.Global));
        }
    }
}
