using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;

namespace Project1.Core.Systems.Effects;

public class EntityEffectWrapper
    : ISaveableNewNew<EntityEffectWrapper>, ISerializableNew<EntityEffectWrapper>
{
    internal SimulationTick StartTick;
    bool _aborted;
    public readonly EffectDef Def;
    public readonly Def Target;
    public readonly float? Budget;
    public readonly int TicksPerUnit;
    public readonly int Duration;
    public readonly float Magnitude;
    public float? RemainingBudget { get; private set; }

    //public bool IsFinished => this.RemainingBudget.HasValue && this.RemainingBudget == 0;
    internal bool IsExpired { get; private set; }
    public bool IsFinished => this._aborted || (this.RemainingBudget.HasValue && this.RemainingBudget == 0);
    //public float Consume(float budget)
    //{
    //    if (!this.RemainingBudget.HasValue)
    //        return budget;
    //    var toConsume = Math.Min(budget, this.RemainingBudget.Value);
    //    this.RemainingBudget -= toConsume;
    //    return toConsume;
    //}
    public bool IsInstant => this.Duration == 0;
    //public bool IsInstant => this.TicksPerUnit == 0;

    public SimulationTick RemainingDuration(SimulationTick now) => this.StartTick + (ulong)this.Duration - now;
    //public TimeSpan RemainingTimespan(SimulationTick now) => TimeSpan.FromMinutes((long)(ulong)this.RemainingDuration(now) / Ticks.PerGameMinute);
    //public TimeSpan RemainingTimespan(SimulationTick now) => TimeSpan.FromMinutes((long)((ulong)this.StartTick + (ulong)Ticks.FromDays(1) - now) / Ticks.PerGameMinute);
    public TimeSpan RemainingTimespan(SimulationTick now) => TimeSpan.FromMinutes((long)((ulong)this.StartTick + (ulong)this.Duration - now) / Ticks.PerGameMinute);
    EntityEffectWrapper(EffectDef def, Def target)
    {
        this.Def = def;
        this.Target = target;
    }
    //public EntityEffectWrapper(EffectDef def, Def target, ulong duration, int magnitude) 
    //    : this(def, target)
    //{
    //    this.Duration = duration;
    //    this.Magnitude = magnitude;
    //}
    public EntityEffectWrapper(EffectDef def, Def target, float? budget, int ticksPerUnit, int duration = 0) 
        : this(def, target)
    {
        this.Budget = budget;
        this.RemainingBudget = budget;
        this.TicksPerUnit = ticksPerUnit;
        this.Duration = duration;
        this.Magnitude = budget.Value;
    }

    internal void Tick(Actor actor)
    {
        var now = actor.World.CurrentTick;
        this.Def.Worker.Tick(actor, this);
        if (now > this.StartTick + (ulong)this.Duration)
            this.IsExpired = true;
    }
    internal void Start(Actor actor)
    {
        this.StartTick = actor.World.CurrentTick;
        this.Def.Worker.Start(actor, this);
    }
    internal void Finish(Actor actor) => this.Def.Worker.Finish(actor, this);
    internal void Abort() => this._aborted = true;

    public void Write(IDataWriter w)
    {
        w.Write(this.Def);
        w.Write(this.Target);
        //w.Write(this.Budget);
        w.Write(this.Budget.HasValue);
        if (this.Budget.HasValue)
            w.Write(this.Budget.Value);

        w.Write(this.TicksPerUnit);
        w.Write(this.StartTick);
        w.Write(this.Duration);
        //w.Write(this.Magnitude);
    }
    public static EntityEffectWrapper Create(IDataReader r)
    {
        var def = r.ReadDef<EffectDef>();
        var target = r.ReadDef();

        int? value = r.ReadBoolean() ? r.ReadInt32() : null;
        var rate = r.ReadInt32();
        var starttick = (SimulationTick)r.ReadUInt64();
        var duration = r.ReadInt32();
        //var mag = r.ReadInt32();
        var fx = new EntityEffectWrapper(def, target, value, rate, duration)
        {
            StartTick = starttick
        };
        return fx;
    }

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        this.Def.Save(tag, "Def");
        this.Target.Save(tag, "Target");
        if(this.RemainingBudget.HasValue)
            tag.Save("Budget", this.RemainingBudget.Value);
        //this.Budget.Save(tag, "Value");
        this.TicksPerUnit.Save(tag, "Rate");
        tag.Save("StartTick", this.StartTick);
        tag.Save("Duration", this.Duration);
        //tag.Save("Magnitude", this.Magnitude);
        return tag;
    }
    public Control GetGui()
    {
        return new Label($"Effect: {this.Def.Name}");
    }
 
    public EntityEffectWrapper Read(IDataReader r) => throw new System.Exception();
    public static EntityEffectWrapper Create(SaveTag tag)
    {
        var def = tag.LoadDef<EffectDef>("Def");
        var target = tag.LoadDef<Def>("Target");
        //var value = tag.LoadInt("Value");
        float? budget = null;
        if (tag.TryLoadSingle("Budget", out var b)) budget = b;
        var rate = tag.LoadInt("Rate");
        var starttick = tag.LoadUlong("StartTick");
        var duration = tag.LoadInt("Duration");
        //var mag = tag.LoadInt("Magnitude");
        return new EntityEffectWrapper(def, target, budget, rate, duration) { StartTick = starttick };
    }

    public override string ToString()
        => EffectsUtils.GetString(this.Def, this.Target);
}
