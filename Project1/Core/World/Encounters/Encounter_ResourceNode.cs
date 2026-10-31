using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Materials;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System;

namespace Project1.Core.World.Encounters;

internal class Encounter_ResourceNode : EncounterWorker
{
    protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    {
        throw new NotImplementedException();
    }

    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
    {
        return 1;
    }

    internal override (WorldActionDef actionDef, WorldActionData data)? DecideAction(FrontierRuntime frontier, Actor actor)
    {
        //return base.DecideAction(frontier, actor);
        return (WorldActionDefOf.HarvestResource, new WorldActionData_HarvestResource(frontier, actor) { MaterialType = MaterialTypeDefOf.Wood });
    }
}
