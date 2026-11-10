using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.World.PointsOfInterest;

internal class Encounter_PoIExit : EncounterWorker
{
    protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    {
        throw new System.NotImplementedException();
    }

    internal override IEnumerable<(EncounterDef encounter, WorldActionData? context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        yield return (EncounterDefOf.PoIExit, new(frontier, actor), ((StaticWorld)actor.World).Frontiers.GetPoi(actor) is not null ? 1 : 0);
    }

    internal override WorldActionDef? DecideAction(WorldActionData context)
    {
        return WorldActionDefOf.ExitPoI;
    }

    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
        => ((StaticWorld)actor.World).Frontiers.GetPoi(actor) is not null ? 1 : 0;
}
