using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Towns.AI.Needs;
using Project1.Core.World.WorldAreas;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Framework.Helpers;
using Project1.Framework.Events;

namespace Project1.Core.Towns.AI.Behaviors;

record struct VisitorDepartingEvent(Actor Actor) : IEventPayload;
sealed class Planner_Departure : Planner
{
    const int MaxTries = 5;
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsHauling)
            return null;
        var decision = actor.AI.Meta.LocationDecision;
        if (!decision.CanEvaluate(actor.Map.World.CurrentTick))
            return null;
        //actor.AI.Meta.LocationDecision.ScheduleNext(actor.World);

        var chance = 1 - actor.Needs.GetPercentage(AdventurerNeedsDefOf.Adventuring);
        var roll = actor.Map.World.Random.Roll(chance);
        // multi step task giver:
        // if targetfrontier is not null, to to map edge and depart
        // if targetfrontier is null, decide which frontier to visit

        if (roll)
        {
            decision.RegisterSuccess();
            var map = actor.Map as StaticMap;
            actor.AI.Meta.SetTargetFrontier(FrontierDefOf.Forest);
            for (int i = 0; i < MaxTries; i++)
            {
                var exit = map.GetRandomEdgeCell().Above;
                if (actor.CanReach(exit))
                {
                    actor.AI.State.Log.Write($"I'm departing for {actor.AI.Meta.TargetFrontier}");
                    actor.Map.Events.Post(new VisitorDepartingEvent(actor));
                    return new Plan(PlanDefOf.Depart, (map, exit));
                }
            }
            actor.Net.Report($"Failed to find a reachable exit for {actor.Name}'s departure");
        }
        decision.RegisterFailure();
        return null;
    }
}
