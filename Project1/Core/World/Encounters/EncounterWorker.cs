using Project1.Core.Entities.Actors;
using Project1.Core.World.WorldAreas;
using Project1.Framework.Helpers;
using System;

#nullable enable

namespace Project1.Core.World.Actions;

internal abstract class EncounterWorker
{
    internal EncounterContext CreateContext(Actor actor)
    {
        var context = this.CreateContextInt();
        context.Actor = actor;
        return context;
    }
    protected virtual EncounterContext CreateContextInt() => new();
    internal void Tick(EncounterRuntime runtime, FrontierRuntime frontier, Actor actor)
    {
        //if (runtime.Percentage == 0)
        //    this.OnBegin(frontier, runtime); 
        // TODO guard against onbegin accidentally called more than once

        //ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(runtime.Percentage, 1);
        if (runtime.Percentage >= 1)
            throw new InvalidOperationException("Encounter has already completed.");
        var prevPercentage = runtime.Percentage;
        runtime.Current += this.GetProgressStep(actor, frontier);
        var nextPercentage = runtime.Percentage;
        this.Tick(runtime, prevPercentage, nextPercentage);
        if (runtime.Percentage >= 1)
            this.Success(frontier, runtime);
    }

    protected abstract void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter);
    void Success(FrontierRuntime frontier, EncounterRuntime encounter)
    {
        if (encounter.Context.Actor.Net.IsServer)
            this.OnSuccess(frontier, encounter);
    }
    internal void Begin(FrontierRuntime frontier, EncounterRuntime encounter)
        => this.OnBegin(frontier, encounter);

    protected virtual void OnBegin(FrontierRuntime frontier, EncounterRuntime encounter) { }

    //internal void Begin(FrontierRuntime frontier, EncounterRuntime encounter) => this.OnBegin(frontier, encounter);
    protected virtual void TickInt(EncounterRuntime encounter, float prevPercentage, float nextPercentage) { }
    void Tick(EncounterRuntime encounter, float prevPercentage, float nextPercentage)
    {
        if (encounter.Context.Actor.Net.IsServer)
            this.TickInt(encounter, prevPercentage, nextPercentage);
    }
    internal abstract int GetWeight(FrontierRuntime frontier, Actor actor);

    //internal virtual EncounterRuntime Start(FrontierRuntime frontier, Actor actor) => null;

    internal virtual EncounterRuntime? TryStart(EncounterDef def, FrontierRuntime frontier, Actor actor)
        => new(def, actor, frontier.Def);

    internal virtual EncounterStates GetState(EncounterRuntime runtime) 
        //=> runtime.Current >= runtime.Max ? EncounterStates.Finished : EncounterStates.InProgress;
        => runtime.Percentage >= 1 ? EncounterStates.Finished : EncounterStates.InProgress;

    internal ProgressInt GetProgress(Actor actor, FrontierDef frontier) => new(1);

    internal virtual int GetProgressMax(EncounterRuntime e) => 1;

    internal virtual float GetProgressStep(Actor actor, FrontierRuntime frontier) => 1;

    internal virtual int GetDurationMultiplier(EncounterContext context) => 1;

    internal virtual string GetTextStatus(EncounterRuntime encounter) => encounter.Def.LabelReadable;

    internal virtual (WorldActionDef actionDef, WorldActionData data)? DecideAction(FrontierRuntime frontier, Actor actor) => null;
}
