using Project1.Core.Entities.Actors;
using Project1.Core.World.WorldAreas;

#nullable enable

namespace Project1.Core.World.PointsOfInterest;

internal class Encounter_PoIExploration : EncounterWorker
{
    internal sealed class Context : EncounterContext
    {
        internal PoIRuntime PoI => field ??= PoISystem.GetPoI(this.Actor)!;

        internal override float GetPercentage(EncounterRuntime e)
            => e.Current / this.PoI.ExplorationTicksRequired;
    }

    protected override Context CreateContextInt() => new();

    static Context Typed(EncounterContext ctx) => (Context)ctx;

    protected override void OnBegin(FrontierRuntime frontier, EncounterRuntime encounter)
    {
        var actor = encounter.Actor;
        var poi = PoISystem.SelectPoI(frontier, actor);
        poi.Add(actor);
    }

    protected override void Tick(EncounterRuntime encounter, float prevPercentage, float nextPercentage)
    {
        var ctx = encounter.Context;
        var poi = Typed(ctx).PoI;
        poi.TryEvent(ctx.Actor, prevPercentage, nextPercentage);
        // TODO award exploration skill xp to actor
    }

    protected override void OnSuccess(FrontierRuntime frontier, Actor actor)
    {
        // TODO produce and apply exploration completion rewards
    }

    internal override float GetProgressStep(Actor actor, FrontierRuntime frontier)
        => 1/* + (actor exploration skill * .01f)*/;

    internal override int GetWeight(FrontierRuntime frontier, Actor actor)
       => frontier.PoIs.Count;

    internal override int GetDurationMultiplier(EncounterContext context)
        => Typed(context).PoI.Size;

    internal override string GetTextStatus(EncounterRuntime encounter)
        => $"Exploring: {Typed(encounter.Context).PoI.Def.LabelReadable}";
}
