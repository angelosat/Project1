using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using Project1.Core.World.Encounters;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

namespace Project1.Core.World.Combat;

//internal sealed class Encounter_Combat : EncounterWorker
//{
//    internal override IEnumerable<(EncounterDef encounter, EncounterContext context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
//        => [(EncounterDefOf.Combat, null, 1)];

//    internal override (WorldActionDef action, WorldActionData data) DecideAction(Actor actor, EncounterContext context)
//        => (WorldActionDefOf.Attack, null);
//        //=> WorldActionWorker_Attack.Create(actor, context.)
//}
