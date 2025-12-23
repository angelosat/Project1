namespace Start_a_Town_
{
    class DeparturePlanner : Planner
    {
        const int MaxTries = 5;
        protected override Plan TryPlan(Actor actor)
        {
            var visitor = actor.GetVisitorProperties();
            var chance = visitor.GetDepartChance();

            var need = actor.GetNeed(AdventurerNeedsDefOf.Adventuring);
            chance = 1 - need.Percentage;

            // multi step task giver:
            // if targetfrontier is not null, to to map edge and depart
            // if targetfrontier is null, decide which frontier to visit

            if (actor.Map.World.Random.Roll(chance))
            {
                var map = actor.Map as StaticMap;
                actor.AI.Meta.TargetFrontier = FrontierDefOf.Forest; // HACK
                for (int i = 0; i < MaxTries; i++)
                {
                    var exit = map.GetRandomEdgeCell().Above;
                    if (actor.CanReach(exit))
                        return new Plan(TaskDefOf.Depart, (map, exit));
                }
                actor.Net.Report($"Failed to find a reachable exit for {actor.Name}'s departure");
            }
            return null;
        }
    }
}
