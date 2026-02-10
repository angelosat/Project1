using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Framework;

namespace Project1.Core.AI.Behaviors.Pathing
{
    class TaskGiverLeaveUnstandableCell : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var cell = actor.Global.ToCell();
            var map = actor.Map;
            if (map.IsStandableIn(cell))
                return null;
            var iterator = cell.GetRadial();
            foreach(var pos in iterator)
            {
                if (!map.IsStandableIn(pos))
                    continue;
                var task = new Plan(PlanDefOf.Moving, pos.At(map));
                return task;
            }
            return null;
        }
    }
}
