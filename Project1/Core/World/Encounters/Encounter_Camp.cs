using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Core.Resources;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;
using System.Diagnostics;

namespace Project1.Core.World.Encounters;

internal sealed class Encounter_Camp : EncounterWorker
{
    protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    {
        throw new UnreachableException();
        var actor = encounter.Actor;
        actor.AI.State.Log.Write($"I have rested a little bit");
        actor.Needs.ApplyAccumulatorDelta(NeedDefOf.Energy, +5);
    }

    internal override WorldActionDef DecideAction(FrontierRuntime frontier, Actor actor)
    {
        return WorldActionDefOf.Rest;
    }

    internal override IEnumerable<(EncounterDef encounter, WorldActionData context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        yield return (EncounterDefOf.Camp, new(frontier, actor), (int)((1 - actor.Resources.GetPercentage(ResourceDefOf.Health)) * 10));
    }

    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
    {
        // this is a formula for the actor to decide to rest autonomously
        // or chance of a resting opportunity during exploring?
        return (int)((1 - actor.Resources.GetPercentage(ResourceDefOf.Health)) * 10);
    }
}
