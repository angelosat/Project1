using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System;

namespace Project1.Core.World.Encounters;

internal class Encounter_Exploration : EncounterWorker
{
    protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    {
        throw new NotImplementedException();
    }

    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
        => (int)((1 - frontier.Exploration.Percentage) * 100);

    internal override (WorldActionDef actionDef, WorldActionData data)? DecideAction(FrontierRuntime frontier, Actor actor)
        => (WorldActionDefOf.Explore, new(frontier, actor));
}
