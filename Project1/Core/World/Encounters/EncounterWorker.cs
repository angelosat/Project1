using Project1.Core.Entities.Actors;
using Project1.Core.World.Encounters;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.World.Actions;

internal abstract class EncounterWorker
{
    internal virtual (WorldActionDef? action, WorldActionData data) DecideAction(Actor actor, EncounterContext context)
        => default;

    internal abstract IEnumerable<(EncounterDef encounter, EncounterContext? context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor);
}
