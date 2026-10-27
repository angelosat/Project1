using Project1.Core.Entities.Actors;
using Project1.Core.World.WorldAreas;

#nullable enable

namespace Project1.Core.World;

internal sealed class EncounterRuntime(OverworldEncounterDef def, Actor actor, FrontierDef frontier)
{
    internal OverworldEncounterDef Def = def;

    //internal ProgressInt Progress = def.Worker.GetProgress(actor, frontier);

    internal readonly int Max = def.Duration;
    internal float Current;
    internal float Percentage => this.Current / this.Max;

    internal EntityRefId ActorId;

    internal void Tick(FrontierRuntime frontier, Actor actor)
    {
        Def.Worker.Tick(this, frontier, actor);
    }

    internal EncounterStates State => Def.Worker.GetState(this);
    //public enum States { InProgress, Paused, Finished };
    //public States State;
    //public float? PercentageComplete => null;
}
