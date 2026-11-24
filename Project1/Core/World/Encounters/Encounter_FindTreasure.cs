using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.World.Encounters;

internal sealed class Encounter_FindTreasure : EncounterWorker
{
    internal override IEnumerable<(EncounterDef encounter, EncounterContext? context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
        => [(EncounterDefOf.FindTreasure, null, frontier.Treasures.Count * 10)];
}
