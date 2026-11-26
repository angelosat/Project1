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
    public readonly EffectDef Def;
    public readonly Def? Target;
    public readonly float? Budget;
    public readonly int TicksPerUnit;
    //public int? Duration { get; init; }
    public readonly float Magnitude;
    public float MagnitudeFinal => this.Magnitude * this.QualityMultiplier;
    public float? RemainingBudget { get; private set; }
    internal bool IsExpired { get; private set; }
    public bool IsFinished => this._aborted || (this.RemainingBudget.HasValue && this.RemainingBudget == 0);
    //SimulationTick Period = (ulong)Ticks.FromSeconds(.5f);
    //public bool IsInstant => this.Def.Duration == 0 && this.Accumulator.Rate == 0;
    public bool IsInstant =>
        this.Duration is { Value: 0 } &&
        this.TickInterval is null;
    public float QualityMultiplier;

    //int TicksRemaining;
    public SimulationDuration? Duration { get; init; }
    public SimulationDuration? TickInterval { get; init; }

    int? TicksRemaining;

    readonly AccumulatorWithRate Accumulator = new();
    float Rate;
    //public SimulationDuration? RemainingDuration(SimulationTick now) => this.StartTick + this.Duration - now;
    public SimulationDuration? RemainingDuration(SimulationTick now)
    {
        if (this.TickInterval is not null)
        {
            if (this.TicksRemaining is not { } ticksRemaining)
                return null;

            float remainingProgress = ticksRemaining - this.Accumulator.Progress;
            int remainingTicks = checked((int)Math.Ceiling(
                remainingProgress / this.Accumulator.Rate));

            return new(remainingTicks);
        }

        if (this.Duration is { } duration)
        {
            var expiration = this.StartTick + duration;

            return now >= expiration
                ? 0
                : expiration - now;
        }

        return null;
    }

    //public TimeSpan RemainingTimespan(SimulationTick now) 
    //    //=> TimeSpan.FromMinutes((long)((ulong)this.StartTick + (ulong)this.Def.BaseDuration - now) / Ticks.PerGameMinute);
    //    => TimeSpan.FromMinutes((this.TotalRemainingSimulationTicks ?? -1) / Ticks.PerGameMinute);

    public TimeSpan? RemainingTimespan(SimulationTick now)
    {
        var remaining = this.RemainingDuration(now);

        return remaining is { } duration
            ? Ticks.ToTimespan(duration.Value)
            : null;
    }

    int? TotalRemainingSimulationTicks
        => this.Duration.HasValue ? (int)((this.TicksRemaining - this.Accumulator.Progress) / this.Accumulator.Rate) : null;

    public int SimulationTicksRemaining
        => (int)((this.TicksRemaining - this.Accumulator.Progress) / this.Accumulator.Rate);

    EntityEffectRuntime(EffectDef def, Def? target)
    {
        this.Def = def;
        this.Target = target;
    }

    public EntityEffectRuntime(EffectDef def,
                               Def? target,
                               float? budget,
                               int ticksPerUnit,
                               //int duration = 0,
                               float multiplier = 1,
                               float rateMultiplier = 1) 
        : this(def, target)
    {
        this.Budget = budget;
        this.RemainingBudget = budget;
        this.TicksPerUnit = ticksPerUnit;
        //this.Duration = def.Duration;
        this.Magnitude = budget ?? 0;
        this.QualityMultiplier = multiplier;
        //this.TicksRemaining = def.TickPeriod?.TickCount ?? 0;
        //var rate = def.TickPeriod.HasValue ? 1f/ def.TickPeriod.Value.Period : 0;
        //this.TicksRemaining = def.TickCount;
        //if(this.TicksRemaining > 0)
        //    this.Accumulator.Rate = rateMultiplier / this.Duration.Value;

        this.Duration = def.Duration;
        this.TickInterval = def.TickInterval;
        this.TicksRemaining = def.TickCount;

        if (this.TickInterval is { } interval)
            this.Accumulator.Rate = 1f / interval.Value;
    }

    //public EntityEffectRuntime(EntityEffectSpec spec)
    //{
    //    this.Def = spec.Def;
    //    this.Target = spec.Target;
    //}

    //public static EntityEffectRuntime CopyFrom(EntityEffectRuntime source)
    //    => new(source.Def, source.Target, source.Budget, source.TicksPerUnit, source.Duration);
    //public EntityEffectRuntime Clone()
    // => new(this.Def, this.Target, this.Budget, this.TicksPerUnit, this.Duration);
    internal void Tick(Actor actor)
    {
        var now = actor.World.CurrentTick;

        if (this.TickInterval is { })
        {
            this.Accumulator.Tick(out var ticks);

            for (int i = 0; i < ticks; i++)
            {
                if (this.TicksRemaining is 0)
                    break;

                this.TicksRemaining--;
                this.Def.Worker.Tick(actor, this);
            }

            if (this.TicksRemaining is 0)
                this.IsExpired = true;
        }
        else if (this.Duration is { } duration)
        {
            if (now >= this.StartTick + duration)
                this.IsExpired = true;
        }
    }
    //internal void Tick(Actor actor)
    //{
    //    var now = actor.World.CurrentTick;
    //    //if((now - this.StartTick) % this.Period == 0)
    //    //    this.Def.Worker.Tick(actor, this);
    //    this.Accumulator.Tick(out var ticks);
    //    for (int i = 0; i < ticks; i++)
    //    {
    //        this.TicksRemaining--;
    //        this.Def.Worker.Tick(actor, this);
    //    }
    //    //if (now > this.StartTick + (ulong)this.Duration)
    //    if (this.Accumulator.Rate == 0 || (this.Def.TickCount > 0 && this.TicksRemaining == 0))
    //    {
    //        //if (now > this.StartTick + (ulong)this.Def.BaseDuration)
    //            this.IsExpired = true;
    //    }
    //}
    internal void Start(Actor actor)
    {
        this.StartTick = actor.World.CurrentTick;
        this.Def.Worker.Start(actor, this);
    }
    //internal void Finish(Actor actor) => this.Def.Worker.Finish(actor, this);
    bool _finishCalled;

    internal void Finish(Actor actor)
    {
        if (this._finishCalled)
            return;

        this._finishCalled = true;
        this.Def.Worker.Finish(actor, this);
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
        if(this.Duration.HasValue)
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
        if(r.ReadBoolean())
            duration = r.ReadInt32();
        var multiplier = r.ReadSingle();
        //var ticksRemaining = r.ReadInt32();
        var accrate = r.ReadSingle();
        //var fx = new EntityEffectRuntime(def, target, value, rate, duration, multiplier)
        //{
        //    StartTick = starttick,
        //};
        var fx = new EntityEffectRuntime(def, target)
        {
            StartTick = starttick,
            Duration = duration,
            //TicksRemaining = ticksRemaining
        };
        fx.Accumulator.Rate = accrate;
        return fx;
    }

    bool IsExpiredAt(SimulationTick now)
    {
        if (this.Def.TickInterval.HasValue)
            return this.Def.TickCount.HasValue && this.TicksRemaining == 0;

        return this.Def.Duration.HasValue
            && now >= this.StartTick + this.Def.Duration.Value;
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
        if(this.Def.Duration.HasValue)
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
        //var duration = tag.LoadInt("Duration");
        //var mag = tag.LoadInt("Magnitude");
        //return new EntityEffectRuntime(def, target, budget, rate, duration) { StartTick = starttick };
        return new EntityEffectRuntime(def, target) { 
            StartTick = starttick,
            Duration = duration,
        };
    }

    internal IEnumerable<(StatDef statDef, float value)> GetStatModifiers(Actor actor)
        => this.Def.Worker.GetStatModifiers(actor);

    public override string ToString()
        => EffectsUtils.GetString(this.Def, this.Target, this.QualityMultiplier);
}

public record struct EntityEffectKey(EffectDef Def, Def? Target);

public record struct EntityEffectSpec(EffectDef Def, Def? Target, int Magnitude)
{
    public EntityEffectSpec(EffectDef Def, int Magnitude) : this(Def, null, Magnitude) { }
}
