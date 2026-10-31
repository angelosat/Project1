using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.Frontiers;
using Project1.Framework.Interfaces;

#nullable enable

namespace Project1.Core.World.Actions;

internal class EncounterContext()
{
    internal Actor Actor;// = actor;
    internal float GetPercentage(EncounterRuntime e) => this.GetCurrent(e) / this.GetMax(e);
    internal virtual float GetMax(EncounterRuntime e) => e.Max;
    internal virtual float GetCurrent(EncounterRuntime e) => e.Current;

    internal FrontierRuntime Frontier => field ??= ((StaticWorld)this.Actor.World).Space.GetFrontier(this.Actor);
}

internal sealed class EncounterRuntime : IProgressBar
{
    internal EncounterDef Def;

    //internal ProgressInt Progress = def.Worker.GetProgress(actor, frontier);

    internal Actor Actor;
    internal EncounterContext Context => field ??= this.Def.Worker.CreateContext(this.Actor);

    internal FrontierDef FrontierDef;

    internal readonly int Max;
    internal float GetMax() => this.Context.GetMax(this);
    internal float GetCurrent() => this.Context.GetCurrent(this);
    public float Percentage => this.Context.GetPercentage(this);// this.Current / this.Max;

    //internal float GetMax() => this.Def.Worker.GetProgressMax(this);
    //internal float GetCurrent() => this.Context.GetCurrent(this);
    //public float Percentage => this.Context.GetPercentage(this);// this.Current / this.Max;

    internal float Current;

    internal EntityRefId ActorId;

    public EncounterRuntime(EncounterDef def, Actor actor, FrontierDef frontier)
    {
        this.Def = def;
        this.Actor = actor;
        //this.Context = def.Worker.CreateContext(actor);
        this.FrontierDef = frontier;
        this.Max = def.DurationInMinutes;// * def.Worker.GetDurationMultiplier(this.Context!);
    }

    internal void Tick(FrontierRuntime frontier, Actor actor)
    {
        this.Def.Worker.Tick(this, frontier, actor);
    }

    public string GetTextStatus() => this.Def.Worker.GetTextStatus(this);

    internal void Begin(FrontierRuntime frontier)
        => this.Def.Worker.Begin(frontier, this);

    internal EncounterStates State => Def.Worker.GetState(this);

    //public enum States { InProgress, Paused, Finished };
    //public States State;
    //public float? PercentageComplete => null;
}

struct EncounterTickResult
{
    internal EncounterRuntime? FollowUpEncounter;
}
