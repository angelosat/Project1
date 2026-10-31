using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.Frontiers;
using Project1.Framework.Interfaces;
using System;

#nullable enable

namespace Project1.Core.World.Actions;

//internal class WorldActionContext()
//{
//    internal Actor Actor;// = actor;
//    internal float GetPercentage(WorldActionRuntime e) => this.GetCurrent(e) / this.GetMax(e);
//    internal virtual float GetMax(WorldActionRuntime e) => e.Max;
//    internal virtual float GetCurrent(WorldActionRuntime e) => e.Current;

//    internal FrontierRuntime Frontier => field ??= ((StaticWorld)this.Actor.World).Space.GetFrontier(this.Actor);
//}

internal sealed class WorldActionRuntime(WorldActionDef def, WorldActionData data) : IProgressBar
{
    internal WorldActionDef Def = def;

    //internal ProgressInt Progress = def.Worker.GetProgress(actor, frontier);

    //internal Actor Actor;
    //internal WorldActionContext Context;// => field ??= this.Def.Worker.CreateContext(this.Actor);

    //internal FrontierDef FrontierDef;

    internal readonly int Max = def.BaseDuration * def.Worker.GetDurationMultiplier(data);
    //internal float GetMax() => this.Context.GetMax(this);
    //internal float GetCurrent() => this.Context.GetCurrent(this);
    //public float Percentage => this.Context.GetPercentage(this);// this.Current / this.Max;
    public float Percentage => this.Current / this.Max;// this.Context.GetPercentage(this);// this.Current / this.Max;

    //internal float GetMax() => this.Def.Worker.GetProgressMax(this);
    //internal float GetCurrent() => this.Context.GetCurrent(this);
    //public float Percentage => this.Context.GetPercentage(this);// this.Current / this.Max;

    internal float Current;

    internal readonly WorldActionData Data = data;

    internal void Tick()
    {
        //this.Def.Worker.Tick(this, frontier, actor);
        if (this.Percentage >= 1)
            throw new InvalidOperationException("Encounter has already completed.");
        //var prevPercentage = this.Percentage;
        this.Current += 1;// this.GetProgressStep(actor, frontier);
        //var nextPercentage = this.Percentage;
        //this.Tick(runtime, prevPercentage, nextPercentage);
        //if (this.Percentage >= 1)
        //    this.Resolve();
    }

    internal void Resolve()
        => this.Def.Worker.Resolve(this.Data);

    internal bool IsFinished => this.Percentage >= 1;
    public Actor Actor => this.Data.Actor;

    //public string GetTextStatus() => this.Def.Worker.GetTextStatus(this);

    //internal void Begin(FrontierRuntime frontier)
    //    => this.Def.Worker.Begin(frontier, this);

    //internal EncounterStates State => Def.Worker.GetState(this);

}
