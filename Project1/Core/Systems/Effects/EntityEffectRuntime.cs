using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Systems.Effects;

public class EntityEffectRuntime
    : ISaveableNewNew<EntityEffectRuntime>, ISerializableNew<EntityEffectRuntime>
{
    internal SimulationTick StartTick;
    bool _aborted;
    public EffectDef Def => this.Dto.Def;
    public EffectData Dto;
    public readonly Def? Target;
    public readonly float? Budget;
    public readonly int TicksPerUnit;
    public readonly float Magnitude;
    public float? RemainingBudget { get; private set; }
    internal bool IsExpired { get; private set; }
    public bool IsFinished => this._aborted || (this.RemainingBudget.HasValue && this.RemainingBudget == 0);

    public bool IsInstant =>
        this.Duration is { Value: 0 } &&
        this.TickInterval is null;
    public float QualityMultiplier;

    internal SimulationDuration? Duration { get; init; }
    internal SimulationDuration? TickInterval { get; init; }

    internal int TicksRemaining;// = IndefiniteTickCount;

    readonly AccumulatorWithRate Accumulator = new();

    public SimulationDuration? GetRemainingDuration()
    {
        if (this.Accumulator.Rate == 0)
            return null;
        float remainingProgress = this.TicksRemaining - this.Accumulator.Progress;
        int remainingTicks = checked((int)Math.Ceiling(
            remainingProgress / this.Accumulator.Rate));

        return new(remainingTicks);
    }

    public TimeSpan? GetRemainingTimespan()
    {
        var remaining = this.GetRemainingDuration();
        return remaining is { } duration
            ? Ticks.ToTimespan(duration.Value)
            : null;
    }

    int? TotalRemainingSimulationTicks
        => this.Duration.HasValue ? (int)((this.TicksRemaining - this.Accumulator.Progress) / this.Accumulator.Rate) : null;

    public int SimulationTicksRemaining
        => (int)((this.TicksRemaining - this.Accumulator.Progress) / this.Accumulator.Rate);

  
    EntityEffectRuntime()
    {
        
    }

    public EntityEffectRuntime(EffectData dto, EffectSchedule schedule)
    {
        this.Dto = dto;
        this.TicksRemaining = schedule.TickCount;
        this.Accumulator.Rate = schedule.TickInterval.IsInfinite ? 0 : 1f / schedule.TickInterval;
    }
    
    internal void Tick(Actor actor)
    {
        this.Accumulator.Tick(out var ticks);

        for (int i = 0; i < ticks; i++)
        {
            if (this.TicksRemaining is 0)
                break;

            this.TicksRemaining--;
            this.Dto.Perform(actor);
        }

        if (this.TicksRemaining is 0)
            this.IsExpired = true;

        // Handle duration-based expiration independently.
    }

    internal void Start(Actor actor)
    {
        this.StartTick = actor.World.CurrentTick;
        this.Dto.Start(actor);
    }

    bool _finishCalled;

    internal void Finish(Actor actor)
    {
        if (this._finishCalled)
            return;

        this._finishCalled = true;
        this.Dto.Finish(actor);
    }

    internal void Abort() => this._aborted = true;

    public void Write(IDataWriter w)
    {
        w.Write(this.Def);
        w.Write(this.Target);
        w.Write(this.Budget.HasValue);
        if (this.Budget.HasValue)
            w.Write(this.Budget.Value);

        w.Write(this.TicksPerUnit);
        w.Write(this.StartTick);
        w.Write(this.Duration.HasValue);
        if (this.Duration.HasValue)
            w.Write(this.Duration.Value);
        w.Write(this.QualityMultiplier);
        //w.Write(this.TicksRemaining);
    }
    public static EntityEffectRuntime Create(IDataReader r)
    {
        var def = r.ReadDef<EffectDef>();
        var target = r.ReadDef();

        int? value = r.ReadBoolean() ? r.ReadInt32() : null;
        var rate = r.ReadInt32();
        var starttick = (SimulationTick)r.ReadUInt64();
        int? duration = null;
        if (r.ReadBoolean())
            duration = r.ReadInt32();
        var multiplier = r.ReadSingle();
        //var ticksRemaining = r.ReadInt32();
        var accrate = r.ReadSingle();
        //var fx = new EntityEffectRuntime(def, target, value, rate, duration, multiplier)
        //{
        //    StartTick = starttick,
        //};
        var fx = new EntityEffectRuntime()
        {
            Dto = null, // HACK
            StartTick = starttick,
            Duration = duration,
            //TicksRemaining = ticksRemaining
        };
        fx.Accumulator.Rate = accrate;
        return fx;
    }

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        this.Def.Save(tag, "Def");
        this.Target.Save(tag, "Target");
        if(this.RemainingBudget.HasValue)
            tag.Save("Budget", this.RemainingBudget.Value);
        this.TicksPerUnit.Save(tag, "Rate");
        tag.Save("StartTick", this.StartTick);
        if(this.Duration.HasValue)
            tag.Save("Duration", this.Duration.Value);
        return tag;
    }
    public Control GetGui()
    {
        return new Label($"Effect: {this.Def.Name}");
    }
 
    public EntityEffectRuntime Read(IDataReader r) => throw new System.Exception();
    public static EntityEffectRuntime Create(SaveTag tag)
    {
        var def = tag.LoadDef<EffectDef>("Def");
        var target = tag.LoadDef<Def>("Target");
        //var value = tag.LoadInt("Value");
        float? budget = null;
        if (tag.TryLoadSingle("Budget", out var b)) budget = b;
        var rate = tag.LoadInt("Rate");
        var starttick = tag.LoadUlong("StartTick");
        int? duration = null;
        if (tag.TryLoadInt("Duration", out var dur))
            duration = dur;
        //return new EntityEffectRuntime(def, target) { 
        //    StartTick = starttick,
        //    Duration = duration,
        //};

        return new EntityEffectRuntime()
        {
            Dto = null, // HACK
            StartTick = starttick,
            Duration = duration,
        };
    }

    internal IEnumerable<(StatDef statDef, float value)> GetStatModifiers()
        => this.Dto.Def.Worker.GetStatModifiers();

    public override string ToString()
    //=> EffectsUtils.GetString(this.Def, this.Target, this.QualityMultiplier);
        {
            //var basestring = $"{this.Dto.Def.Verb} {(int)(effect.BaseMagnitude * multiplier)} {target?.LabelReadable}";
            var basestring = $"{this.Dto.Def.Verb} {this.Dto.Def.LabelReadable}"; // TODO
            if (this.Duration is { } dur)
                if (dur > 0)
                    //basestring += $" for {TimeSpan.FromMinutes(Ticks.PerGameMinute * effect.BaseDuration)}";
                    basestring += $" for {Ticks.ToString(dur)}";
            return basestring;
        }

    internal string GetString()
        => this.Dto.GetString();
}

public record struct EntityEffectKey(EffectDef Def, Def? Target);

public record struct EntityEffectSpec(EffectDef Def, Def? Target, int Magnitude)
{
    public EntityEffectSpec(EffectDef Def, int Magnitude) : this(Def, null, Magnitude) { }
}
