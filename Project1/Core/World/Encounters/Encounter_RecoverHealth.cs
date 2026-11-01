using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

namespace Project1.Core.World.Encounters;

internal sealed class Encounter_RecoverHealth : EncounterWorker
{
    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
        => 0;// (int)((1-actor.Resources.GetPercentage(ResourceDefOf.Health)) * 10);

    protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    {
        var actor = encounter.Actor;
        actor.Resources.ApplyDelta(ResourceDefOf.Health, 5);
    }

    internal override IEnumerable<(EncounterDef encounter, WorldActionData context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        yield return (EncounterDefOf.RecoverHealth, new(frontier, actor), 0);
    }
}
