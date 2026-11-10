using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System;
using System.Collections.Generic;

namespace Project1.Core.World.Encounters;

internal class Encounter_Exploration : EncounterWorker
{
    protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    {
        throw new NotImplementedException();
    }

    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
        => (int)((1 - frontier.Exploration.Percentage) * 100);

    internal override WorldActionDef DecideAction(WorldActionData context)
        => WorldActionDefOf.Explore;

    internal override IEnumerable<(EncounterDef encounter, WorldActionData context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        yield return (EncounterDefOf.Exploration, new(frontier, actor), (int)((1 - frontier.Exploration.Percentage) * 100));
    }
}
