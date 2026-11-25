using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

namespace Project1.Core.World.Encounters;

internal sealed class Encounter_Camp : EncounterWorker
{
    internal override WorldActionData DecideAction(Actor actor, EncounterContext context)
        => new WorldActionData_Default(actor, WorldActionDefOf.Rest);

    internal override IEnumerable<(EncounterDef encounter, EncounterContext context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
        => [(EncounterDefOf.Camp, null, (int)((1 - actor.Resources.GetPercentage(ResourceDefOf.Health)) * 10))];
}
