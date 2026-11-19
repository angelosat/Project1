using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System.Collections.Generic;

namespace Project1.Core.World.Encounters;

internal sealed class Encounter_Injury : EncounterWorker
{
    //protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    //{
    //    var actor = encounter.Actor;
    //    var dmgBase = frontier.Def.Tier * 2;
    //    var dmg = dmgBase + actor.World.Random.Next(-dmgBase, dmgBase) / 2;
    //    //dmg += 50;
    //    actor.Resources.ApplyDelta(ResourceDefOf.Health, -dmg);
    //    actor.World.Events.Post(new ActorOffmapHealthEvent(actor, dmg));
    //    $"{actor} received {dmg} damage".ToConsole();
    //}
    //internal override int GetWeight(FrontierRuntime frontier, Actor actor)
    //{
    //    return frontier.Def.Tier;
    //}

    internal override WorldActionDef DecideAction(WorldActionData context)
    {
        return WorldActionDefOf.AvoidInjury;
    }

    internal override IEnumerable<(EncounterDef encounter, WorldActionData context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        yield return (EncounterDefOf.Injury, new(actor), frontier.Def.Tier);
    }
}
