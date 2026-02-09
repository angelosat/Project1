using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Towns
{
    class PlannerWorkplace : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            return actor.Workplace?.GetTask(actor);
        }
    }
}
