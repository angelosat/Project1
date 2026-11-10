using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Materials;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System;
using System.Collections.Generic;

namespace Project1.Core.World.Encounters;

internal class Encounter_ResourceNode : EncounterWorker
{
    static readonly List<MaterialRefinementDef> ValidItems = [MaterialRefinementDefOf.Logs, MaterialRefinementDefOf.Ore];

    protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    {
        throw new NotImplementedException();
    }

    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
    {
        int targetCount = 10;
        return 1;
    }

    internal override IEnumerable<(EncounterDef encounter, WorldActionData context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        foreach (var ((matRef, mat), weight) in frontier.Def.ResourceTypes)
            yield return (EncounterDefOf.Resource, new WorldActionData_HarvestResource(frontier, actor) { Refinement = matRef, Material = mat }, weight);
    }

    //internal override (WorldActionDef actionDef, WorldActionData data)? DecideAction(FrontierRuntime frontier, Actor actor)
    //{
    //    //return base.DecideAction(frontier, actor);
    //    return (WorldActionDefOf.HarvestResource, new WorldActionData_HarvestResource(frontier, actor) { MaterialType = MaterialTypeDefOf.Wood });
    //}

    internal override WorldActionDef DecideAction(WorldActionData context)
    {
        return WorldActionDefOf.HarvestResource;
    }
}
