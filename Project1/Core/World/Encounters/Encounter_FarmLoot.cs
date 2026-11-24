using Project1.Core.AI.MetaRoles.Adventurer;
using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

namespace Project1.Core.World.Encounters;

internal sealed class Encounter_FarmLoot : EncounterWorker
{
    internal override IEnumerable<(EncounterDef encounter, EncounterContext context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        if (actor.AI.GetMeta<RoleAdventurerData>() is not RoleAdventurerData adventurer)
            yield break;
        var desiredLoot = adventurer.NextDesiredLoot;
        var weight = desiredLoot.HasValue ? 1 : 0;
        yield return (EncounterDefOf.FarmLoot, null, weight);
    }
}
