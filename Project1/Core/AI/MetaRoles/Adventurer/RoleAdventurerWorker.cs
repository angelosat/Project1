using Project1.Core.Entities.Actors;
using Project1.Core.Towns.AI.Needs;
using Project1.Core.World.WorldAreas;
using Project1.Framework.Helpers;
using System.Linq;

namespace Project1.Core.AI.MetaRoles.Adventurer;

internal abstract class FrontierDecider
{
    internal abstract (FrontierDef frontier, int score) GetScore(AIComponent comp);
}
internal sealed class FrontierDecider_FromItem : FrontierDecider
{
    internal override (FrontierDef frontier, int score) GetScore(AIComponent comp)
    {
        var meta = comp.GetMeta<RoleAdventurerData>();
        var desire = meta.NextDesiredLoot;
        if (!desire.HasValue)
            return default;
        return (FrontierManager.GetFrontier(desire.Value.matdef.Tier), 100);
    }
}
internal sealed class FrontierDecider_ReturnToTown : FrontierDecider
{
    internal override (FrontierDef frontier, int score) GetScore(AIComponent comp)
    {
        var meta = comp.GetMeta<RoleAdventurerData>();
        var desire = meta.NextDesiredLoot;
        if (!desire.HasValue)
            return default;
        var need = comp.Owner.Needs.GetValue(AdventurerNeedsDefOf.Adventuring);
        return (null, need);
    }
}
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
        meta.LocationDecision.ScheduleNext(world);
    }

    private static void DecideFrontier(Actor actor, Simulation.WorldBase world, RoleAdventurerData meta)
    {
        if (!meta.LocationDecision.CanEvaluate(world.CurrentTick))
            return;
        meta.LocationDecision.ScheduleNext(world);
        if (meta.TargetFrontier is null) // actor is already returning to town
            return;
        var frontier = FrontierManager.Deciders.Select(d => d.GetScore(actor.AI)).MaxBy(i => i.score).frontier;
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
