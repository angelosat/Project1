using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using Project1.Core.World.Encounters;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

namespace Project1.Core.World.Combat;

internal sealed class Encounter_Combat : EncounterWorker
{

    //sealed class Context : EncounterContext
    //{
    //    internal CombatRuntime Combat => field ??= ((StaticWorld)this.Actor.World).Combat.Get(this.Actor);
    //    internal override float GetMax(EncounterRuntime e)
    //        => 1;
    //}

    internal override IEnumerable<(EncounterDef encounter, WorldActionData context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
        => [(EncounterDefOf.Combat, new(actor), 1)];

    internal override WorldActionDef DecideAction(WorldActionData context)
    {
        return WorldActionDefOf.Attack;
    }

    internal override IEnumerable<(EncounterDef encounter, EncounterContext context, int weight)> GetCandidatesNew(FrontierRuntime frontier, Actor actor)
        => [(EncounterDefOf.Combat, null, 1)];

    internal override (WorldActionDef action, WorldActionData data) DecideActionNew(Actor actor, EncounterContext context)
        => (WorldActionDefOf.Attack, null);
}
