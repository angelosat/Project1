using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Towns.AI.Needs;
using Project1.Core.World.WorldAreas;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Simulation;

namespace Project1.Core.Towns.AI.Behaviors
{
    class PlannerDeparture : Planner
    {
        const int MaxTries = 5;
        protected override Plan TryPlan(Actor actor)
        {
            //var visitor = actor.GetVisitorProperties();
            //var chance = visitor.GetDepartChance();

            var need = actor.GetNeed(AdventurerNeedsDefOf.Adventuring);
            var chance = 1 - need.Percentage;

            // multi step task giver:
            // if targetfrontier is not null, to to map edge and depart
            // if targetfrontier is null, decide which frontier to visit

            if (actor.Map.World.Random.Roll(chance))
            {
                var map = actor.Map as StaticMap;
                //actor.AI.Meta.TargetFrontier = FrontierDefOf.Forest; // HACK
                actor.AI.Meta.SetTargetFrontier(FrontierDefOf.Forest);
                actor.AI.Meta.LocationDecision.ScheduleNext(actor.World);
                for (int i = 0; i < MaxTries; i++)
                {
                    var exit = map.GetRandomEdgeCell().Above;
                    if (actor.CanReach(exit))
                    {
                        //AILog.SyncWrite(actor, $"I'm departing for {actor.AI.Meta.TargetFrontier}");
                        actor.AI.State.Log.Write($"I'm departing for {actor.AI.Meta.TargetFrontier}");
                        return new Plan(PlanDefOf.Depart, (map, exit));
                    }
                }
                actor.Net.Report($"Failed to find a reachable exit for {actor.Name}'s departure");
            }
            return null;
        }
    }
}
