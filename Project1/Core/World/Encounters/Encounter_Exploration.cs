using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

namespace Project1.Core.World.Encounters;

internal sealed class Encounter_Exploration : EncounterWorker
{
    internal override (WorldActionDef action, WorldActionData data) DecideAction(Actor actor, EncounterContext context)
        => WorldActionWorker_Exploration.Create(actor);// (WorldActionDefOf.Explore, null);

    internal override IEnumerable<(EncounterDef encounter, EncounterContext context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
        => [(EncounterDefOf.Exploration, null, (int)((1 - frontier.Exploration.Percentage) * 100))];
}
