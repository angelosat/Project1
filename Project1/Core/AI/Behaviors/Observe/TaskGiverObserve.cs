using System.Collections.Generic;
using System.Linq;
using Project1.Framework.Helpers;
using Project1.Core.Entities;
using Project1.Core.Needs;
using Project1.Core.Entities.Actors;
using Project1.Core.AI.Reservations;

namespace Project1.Core.AI.Behaviors.Observe
{
    class TaskGiverObserve : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var need = actor.GetNeed(NeedDefOf.Curiosity);
            if (need.Value > 50)
                return null;
            var potentialTargets = actor.Map.GetEntities()
                .Where(o=>actor.CanReserve(o));
            var randomized = new Queue<GameObject>(potentialTargets.Shuffle(actor.Map.Random));

            while (randomized.Count > 0)
            {
                var obj = randomized.Dequeue();
                if (obj == actor)
                    continue;
                return new Plan(typeof(BehaviorTaskObserveNew)) { TargetA = new TargetArgs(obj) };
            }
            return null;
        }
    }
}
