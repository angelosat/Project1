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
    //internal sealed class Context : EncounterContext
    //{
    //    internal PoIRuntime PoI => field ??= PoISystem.GetPoI(this.Actor)!;

    //    //internal override float GetPercentage(EncounterRuntime e)
    //    //=> e.Current / this.PoI.ExplorationTicksRequired;
    //    internal override float GetMax(EncounterRuntime e)
    //        => this.PoI.ExplorationTicksRequired;
    //}

    //protected override Context CreateContextInt() => new();

    //static Context Typed(EncounterContext ctx) => (Context)ctx;

    //internal override int GetProgressMax(EncounterRuntime e)
    //    => Typed(e.Context).PoI.ExplorationTicksRequired;

    //protected override void OnBegin(FrontierRuntime frontier, EncounterRuntime encounter)
    //{
    //    throw new UnreachableException();
    //    //var actor = encounter.Actor;
    //    //var poi = PoISystem.SelectPoI(frontier, actor);
    //    //poi.Add(actor);
    //}

    //protected override void TickInt(EncounterRuntime encounter, float prevPercentage, float nextPercentage)
    //{
    //    var ctx = encounter.Context;
    //    var poi = Typed(ctx).PoI;
    //    poi.TryEvent(ctx.Actor, prevPercentage, nextPercentage);
    //    // TODO award exploration skill xp to actor
    //}

    //protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    //{
    //    // TODO produce and apply exploration completion rewards
    //    Typed(encounter.Context).PoI.Remove(encounter.Context.Actor);
    //}

    //internal override float GetProgressStep(Actor actor, FrontierRuntime frontier)
    //    => 1/* + (actor exploration skill * .01f)*/;

    //internal override int GetWeight(FrontierRuntime frontier, Actor actor)
    //   => frontier.PoIs.Count * 50;

    //internal override int GetDurationMultiplier(EncounterContext context)
    //    => Typed(context).PoI.Size;

    //internal override string GetTextStatus(EncounterRuntime encounter)
    //    => $"Exploring: {Typed(encounter.Context).PoI.Def.LabelReadable}";

    internal override WorldActionDef DecideAction(WorldActionData context)
    {
        //var poi = PoISystem.SelectPoI(frontier, actor);
        //var data = new WorldActionData_PoI(frontier, actor) { PoI = poi };
        return WorldActionDefOf.EnterPoI;
    }

    internal override (WorldActionDef? action, WorldActionData data) DecideActionNew(Actor actor, EncounterContext context)
        => WorldActionWorker_PoIEnter.Create(actor, ((Context)context).PoiId);

    internal override IEnumerable<(EncounterDef encounter, WorldActionData? context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        throw new System.Exception();
        //var poi = PoISystem.SelectPoI(frontier, actor);
        //yield return (EncounterDefOf.PoIEntrance, new WorldActionData_PoI(actor) { PoIId = poi.id }, frontier.PoIs.Count);// * 100);
    }

    internal override IEnumerable<(EncounterDef encounter, EncounterContext? context, int weight)> GetCandidatesNew(FrontierRuntime frontier, Actor actor)
    {
        var poi = PoISystem.SelectPoI(frontier, actor);
        yield return (EncounterDefOf.PoIEntrance, new Context(poi.id), frontier.PoIs.Count);// * 100);
    }
}
