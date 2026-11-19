using Project1.Core.Entities.Actors;
using Project1.Framework.Interfaces;
using System;

#nullable enable

namespace Project1.Core.World.Actions;

internal sealed class WorldActionRuntime(WorldActionDef def, WorldActionData data) : IProgressBar
{
    internal WorldActionDef Def = def;

    internal readonly int Max = (int)(def.BaseDuration * def.Worker.GetDurationMultiplier(data));
    
    public float Percentage => this.Current / this.Max;

    internal float Current;

    internal readonly WorldActionData Data = data;

    internal void Tick()
    {
        if (this.Percentage >= 1)
            throw new InvalidOperationException("Encounter has already completed.");
        this.Current += 1;
    }

    internal void Resolve()
        => this.Def.Worker.Resolve(this.Data);

    public string GetStatusText()
        => this.Def.Worker.GetStatusText(this);

    internal bool IsFinished => this.Percentage >= 1;
    public Actor Actor => this.Data.Actor;
}
