using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using Project1.Core.World.Encounters;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.World.PointsOfInterest;

internal sealed class Encounter_PoIEntrance : EncounterWorker
{
    sealed record Context(int PoiId) : EncounterContext;

    internal override WorldActionData DecideAction(Actor actor, EncounterContext context)
        => new WorldActionData_PoiEnter(actor) { PoIId = ((Context)context).PoiId };

    internal override IEnumerable<(EncounterDef encounter, EncounterContext? context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        var poi = PoISystem.SelectPoI(frontier, actor);
        yield return (EncounterDefOf.PoIEntrance, new Context(poi.id), frontier.PoIs.Count);// * 100);
    }
}
