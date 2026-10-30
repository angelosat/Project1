using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.WorldAreas;

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
}
