using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

namespace Project1.Core.World.Combat;

internal class Encounter_Combat : EncounterWorker
{
    sealed class Context : EncounterContext
    {
        internal CombatRuntime Combat => field ??= ((StaticWorld)this.Actor.World).Combat.Get(this.Actor);
        internal override float GetMax(EncounterRuntime e)
            => 1;
    }

    protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    {
    }

    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
        => 0;

    internal override IEnumerable<(EncounterDef encounter, WorldActionData context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
        => [(EncounterDefOf.Combat, new(frontier, actor), 1)];

    internal override WorldActionDef DecideAction(FrontierRuntime frontier, Actor actor)
    {
        return WorldActionDefOf.Attack;
    }
}
