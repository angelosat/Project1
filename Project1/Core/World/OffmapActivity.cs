using Project1.Core.Entities.Actors;
using Project1.Core.World.WorldAreas;
using Project1.Framework.Helpers;
using System;

#nullable enable

namespace Project1.Core.World;

internal abstract class OffmapActivity
{
    internal void Tick(EncounterRuntime runtime, FrontierRuntime frontier, Actor actor)
    {
        //ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(runtime.Percentage, 1);
        if (runtime.Percentage >= 1)
            throw new InvalidOperationException("Encounter has already completed.");
        runtime.Current += this.GetProgressStep(actor, frontier);
        if (runtime.Percentage >= 1)
            this.OnSuccess(frontier, actor);
    }

    protected abstract void OnSuccess(FrontierRuntime frontier, Actor actor);
    //protected virtual void OnBegin(FrontierRuntime frontier, Actor actor) { }

    internal abstract int GetWeight(FrontierRuntime frontier, Actor actor);

    internal virtual EncounterRuntime Start(FrontierRuntime frontier, Actor actor) => null;

    internal virtual EncounterStates GetState(EncounterRuntime runtime) 
        => runtime.Current >= runtime.Max ? EncounterStates.Finished : EncounterStates.InProgress;

    internal ProgressInt GetProgress(Actor actor, FrontierDef frontier) => new(1);

    internal virtual int GetProgressMax(Actor actor, FrontierDef frontier) => 1;

    internal virtual float GetProgressStep(Actor actor, FrontierRuntime frontier) => 1;
}
