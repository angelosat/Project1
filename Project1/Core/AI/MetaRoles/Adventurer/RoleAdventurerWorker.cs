using Project1.Core.Entities.Actors;
using Project1.Core.Towns.AI.Needs;
using Project1.Core.World.WorldAreas;
using Project1.Framework.Helpers;
using System.Linq;

namespace Project1.Core.AI.MetaRoles.Adventurer;

internal class RoleAdventurerWorker : RoleMetaWorker
{
    internal override void Tick(RoleMetaWrapper meta)
    {
        var actor = meta.Actor;
        if (actor.Net.IsClient)
            return;

        var world = actor.World;

        var typedMeta = (RoleAdventurerData)meta;
        DecideFrontier(actor, world, typedMeta);
        return;
        var roll = world.Random.Roll(actor.Needs.GetPercentage(AdventurerNeedsDefOf.Adventuring));
        if (roll)
        {
            meta.LocationDecision.RegisterSuccess();
            actor.AI.Meta.ReturnToTown();
            actor.AI.State.Log.Write("I'm returning to town.");
        }
        else
        {
            meta.LocationDecision.RegisterFailure();
            actor.AI.State.Log.Write("I'll stay out adventuring some more.");

            //var typedMeta = (RoleAdventurerData)meta;
            //var frontier = FrontierManager.Deciders.SelectMany(d => d.GetScore(actor.AI)).OrderByDescending(f => f.score).FirstOrDefault().frontier;
            //if(frontier != null)
            //    typedMeta.SetTargetFrontier(frontier);

        }
        //meta.LocationDecision.ScheduleNext(world);
    }

    private static void DecideFrontier(Actor actor, Simulation.WorldBase world, RoleAdventurerData meta)
    {
        if (!meta.LocationDecision.CanEvaluate(world.CurrentTick))
            return;
        //meta.LocationDecision.ScheduleNext(world);
        if (meta.TargetFrontier is null) // actor is already returning to town
            return;
        var scored = FrontierManager.Deciders.Select(d => d.GetScore(actor.AI));
        var best = scored.MaxBy(i => i.score);
        if (best.score <= 0)
            return;

        var candidates = scored.Where(a => a.score > 0).OrderBy(a => a.score).ToArray();
        //var sum = candidates.Sum(a => a.score);
        var roll = world.Random.Next(100);
        var found = candidates.FirstOrDefault(c => roll <= c.score);
       
        //var frontier = best.frontier;
        var frontier = found.decider is not null ? found.frontier : meta.TargetFrontier;
        if (frontier is null)
        {
            meta.ReturnToTown();
            actor.AI.State.Log.Write("I'm returning to town.");
        }
        else if (frontier == meta.TargetFrontier)
        {
            actor.AI.State.Log.Write($"I'm staying at {frontier.LabelReadable}.");
        }
        else
        {
            actor.AI.State.Log.Write($"I'm going to {frontier.LabelReadable}.");
            meta.SetTargetFrontier(frontier);
        }
    }
}
