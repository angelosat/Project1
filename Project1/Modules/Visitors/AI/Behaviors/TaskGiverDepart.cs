using Start_a_Town_.Core;

namespace Start_a_Town_
{
    class TaskGiverDepart : Planner
    {
        const int MaxTries = 5;
        protected override Plan TryPlan(Actor actor)
        {
            var visitor = actor.GetVisitorProperties();
            var chance = visitor.GetDepartChance();
            if (actor.Map.World.Random.Chance(chance))
            {
                var map = actor.Map as StaticMap;
                for (int i = 0; i < MaxTries; i++)
                {
                    var exit = map.GetRandomEdgeCell().Above();
                    if (actor.CanReach(exit))
                        return new Plan(TaskDefOf.Depart, (map, exit));
                }
                actor.Net.Report($"Failed to find a reachable exit for {actor.Name}'s departure");
            }
            return null;
        }
    }
}
