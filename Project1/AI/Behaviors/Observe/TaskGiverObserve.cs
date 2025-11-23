using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.AI.Behaviors.Observe
{
    class TaskGiverObserve : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var need = actor.GetNeed(NeedDef.Curiosity);
            if (need.Value > 50)
                return null;
            var potentialTargets = actor.Map.GetObjects()
                .Where(o=>actor.CanReserve(o));
            var randomized = new Queue<GameObject>(potentialTargets.Shuffle(actor.Map.Random));

            while (randomized.Count > 0)
            {
                var obj = randomized.Dequeue();
                if (obj == actor)
                    continue;
                return new AITask(typeof(BehaviorTaskObserveNew)) { TargetA = new TargetArgs(obj) };
            }
            return null;
        }
    }
}
