using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Materials;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

namespace Project1.Core.World.Encounters;

internal class Encounter_ResourceNode : EncounterWorker
{
    sealed record Context(MaterialRefinementDef Refinement, MaterialDef Material) : EncounterContext;

    static readonly List<MaterialRefinementDef> ValidItems = [MaterialRefinementDefOf.Logs, MaterialRefinementDefOf.Ore];

    internal override IEnumerable<(EncounterDef encounter, EncounterContext context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        foreach (var ((matRef, mat), weight) in frontier.Def.ResourceTypes)
            yield return (EncounterDefOf.Resource, new Context(matRef, mat), weight);
    }

    internal override (WorldActionDef action, WorldActionData data) DecideAction(Actor actor, EncounterContext context)
    {
        var typed = (Context)context;
        return WorldActionWorker_HarvestResource.Create(actor, typed.Refinement, typed.Material);
    }
}
