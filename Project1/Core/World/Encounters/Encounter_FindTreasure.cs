using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

namespace Project1.Core.World.Encounters;

internal sealed class Encounter_FindTreasure : EncounterWorker
{
    protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    {
        var actor = encounter.Actor;
        var rand = actor.World.Random;
        if (!frontier.TryFindTreasure(rand, actor, out var treasure))
        {
            actor.AI.State.Log.Write($"Searched for treasure but found nothing");
            return;
        }
        actor.Inventory.Insert(treasure);
        actor.AI.State.Log.Write($"Treasure found! [{treasure.Name}]");
    }

    internal override IEnumerable<(EncounterDef encounter, WorldActionData context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        yield return (EncounterDefOf.FindTreasure, new(frontier, actor), frontier.Treasures.Count);
    }

    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
    {
        return frontier.Treasures.Count;
    }
}
