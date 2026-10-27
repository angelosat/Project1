using Project1.Core.Entities.Actors;
using Project1.Core.World.WorldAreas;
using Project1.Framework.Helpers;

#nullable enable

namespace Project1.Core.World;

internal abstract class OffmapActivity
{
    internal void Tick(EncounterRuntime runtime, FrontierRuntime frontier, Actor actor)
    {
        this.TickInt(frontier, actor);
        runtime.Current += this.GetProgressStep(actor, frontier);
    }

    protected abstract void TickInt(FrontierRuntime frontier, Actor actor);

    internal abstract int GetWeight(FrontierRuntime frontier, Actor actor);

    internal virtual EncounterRuntime Start(FrontierRuntime frontier, Actor actor) => null;

    internal virtual EncounterStates GetState(EncounterRuntime runtime) 
        => runtime.Current >= runtime.Max ? EncounterStates.Finished : EncounterStates.InProgress;

    internal ProgressInt GetProgress(Actor actor, FrontierDef frontier) => new(1);

    internal virtual int GetProgressMax(Actor actor, FrontierDef frontier) => 1;

    internal virtual float GetProgressStep(Actor actor, FrontierRuntime frontier) => 1;
}
