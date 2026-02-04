using Project1.Framework.Entities.Actors;
using Start_a_Town_;

namespace Project1.Core.Tavern
{
    class TaskGiverWorkplace : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            return actor.Workplace?.GetTask(actor);
        }
    }
}
