using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;

#nullable enable

namespace Project1.Core.World.PointsOfInterest;

internal class Encounter_PoIExploration : EncounterWorker
{
    internal sealed class Context : EncounterContext
    {
        internal PoIRuntime PoI => field ??= PoISystem.GetPoI(this.Actor)!;

        //internal override float GetPercentage(EncounterRuntime e)
        //=> e.Current / this.PoI.ExplorationTicksRequired;
        internal override float GetMax(EncounterRuntime e)
            => this.PoI.ExplorationTicksRequired;
    }

    protected override Context CreateContextInt() => new();

    static Context Typed(EncounterContext ctx) => (Context)ctx;

    //internal override int GetProgressMax(EncounterRuntime e)
    //    => Typed(e.Context).PoI.ExplorationTicksRequired;

    protected override void OnBegin(FrontierRuntime frontier, EncounterRuntime encounter)
    {
        var actor = encounter.Actor;
        var poi = PoISystem.SelectPoI(frontier, actor);
        poi.Add(actor);
    }

    protected override void TickInt(EncounterRuntime encounter, float prevPercentage, float nextPercentage)
    {
        var ctx = encounter.Context;
        var poi = Typed(ctx).PoI;
        poi.TryEvent(ctx.Actor, prevPercentage, nextPercentage);
        // TODO award exploration skill xp to actor
    }

    protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    {
        // TODO produce and apply exploration completion rewards
        Typed(encounter.Context).PoI.Remove(encounter.Context.Actor);
    }

    internal override float GetProgressStep(Actor actor, FrontierRuntime frontier)
        => 1/* + (actor exploration skill * .01f)*/;

    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
       => frontier.PoIs.Count;// * 50;

    internal override int GetDurationMultiplier(EncounterContext context)
        => Typed(context).PoI.Size;

    internal override string GetTextStatus(EncounterRuntime encounter)
        => $"Exploring: {Typed(encounter.Context).PoI.Def.LabelReadable}";

    internal override (WorldActionDef actionDef, WorldActionData data)? DecideAction(FrontierRuntime frontier, Actor actor)
    {
        var poi = PoISystem.SelectPoI(frontier, actor);
        var data = new WorldActionData_PoI(frontier, actor) { PoI = poi };
        return (WorldActionDefOf.EnterPoI, data);
    }
}
