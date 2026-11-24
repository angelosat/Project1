using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.Actions;
using Project1.Core.World.Encounters;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.World.PointsOfInterest;

internal class Encounter_PoIExit : EncounterWorker
{
    internal override IEnumerable<(EncounterDef encounter, EncounterContext? context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
        => [(EncounterDefOf.PoIExit, null, ((StaticWorld)actor.World).Frontiers.GetPoi(actor) is not null ? 1 : 0)];
}
