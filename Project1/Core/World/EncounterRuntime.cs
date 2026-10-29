using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.WorldAreas;
using System;

#nullable enable

namespace Project1.Core.World;

internal class EncounterContext()
{
    internal Actor Actor;// = actor;
    internal virtual float GetPercentage(EncounterRuntime e) => e.Current / e.Max;

    internal FrontierRuntime Frontier => field ??= ((StaticWorld)this.Actor.World).Space.GetFrontier(this.Actor);
}

internal sealed class EncounterRuntime
{
    internal EncounterDef Def;

    //internal ProgressInt Progress = def.Worker.GetProgress(actor, frontier);

    internal Actor Actor;
    internal EncounterContext Context => field ??= this.Def.Worker.CreateContext(this.Actor);

    internal FrontierDef FrontierDef;

    internal readonly int Max;
    internal float Current;
    internal float Percentage => this.Context.GetPercentage(this);// this.Current / this.Max;

    internal EntityRefId ActorId;

    public EncounterRuntime(EncounterDef def, Actor actor, FrontierDef frontier)
    {
        this.Def = def;
        this.Actor = actor;
        //this.Context = def.Worker.CreateContext(actor);
        this.FrontierDef = frontier;
        //this.Max = def.DurationInMinutes * def.Worker.GetDurationMultiplier(this.Context!);
    }

    internal void Tick(FrontierRuntime frontier, Actor actor)
    {
        Def.Worker.Tick(this, frontier, actor);
    }

    public string GetTextStatus() => this.Def.Worker.GetTextStatus(this);

    internal void Begin(FrontierRuntime frontier)
        => this.Def.Worker.Begin(frontier, this);

    internal EncounterStates State => Def.Worker.GetState(this);
    //public enum States { InProgress, Paused, Finished };
    //public States State;
    //public float? PercentageComplete => null;
}
