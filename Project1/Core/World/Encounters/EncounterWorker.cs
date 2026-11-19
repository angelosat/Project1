using Project1.Core.Entities.Actors;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.World.Actions;

internal abstract class EncounterWorker
{
    internal virtual WorldActionDef? DecideAction(WorldActionData context) => null;

    internal abstract IEnumerable<(EncounterDef encounter, WorldActionData? context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor);
}
