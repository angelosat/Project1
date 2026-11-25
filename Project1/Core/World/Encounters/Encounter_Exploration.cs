using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

namespace Project1.Core.World.Encounters;

internal sealed class Encounter_Exploration : EncounterWorker
{
    internal override WorldActionData DecideAction(Actor actor, EncounterContext context)
        => new WorldActionData_Default(actor, WorldActionDefOf.Explore);// (WorldActionDefOf.Explore, null);

    internal override IEnumerable<(EncounterDef encounter, EncounterContext context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
        => [(EncounterDefOf.Exploration, null, (int)((1 - frontier.Exploration.Percentage) * 100))];
}
