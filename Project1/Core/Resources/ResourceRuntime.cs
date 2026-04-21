using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.UI.NamePlates;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Interfaces;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Resources;

public sealed class ResourceRuntime : IProgressBar, ISaveableNewNew<ResourceRuntime>, IDefWrapper<ResourceDef>, ISerializableNew<ResourceRuntime>, INamed
{
    public Entity Owner;
    public ResourceDef ResourceDef;
    public List<ResourceRateModifier> Modifiers = new();
    public int TicksPerRecoverOne, TicksPerDrainOne;
    int _max;
    internal AccumulatorWithRate Accumulator = new();
    public ResourceRuntime()
    {

    }
    public int Max
    {
        get => this._max; set
        {
            var oldmax = this._max;
            this._max = value;
            this.Value = Math.Min(this.Value, this._max);
        }
    }
    int _value;
    int _overflow, _overflowMax;
    public int Overflow
    {
        get => this._overflow;
        set => this._overflow = Math.Max(0, Math.Min(this._overflowMax, value));
    }
    public int Value
    {
        get => this._value;
        set
        {
            this._value = this.ApplyValue(value, out var overflow);
            this._overflow = Math.Min(overflow, this._overflowMax);
        }
    }
    public int ValueWithOverflow => this._value + this._overflow;
    public int MaxWithOverflow => this._max + this._overflowMax;
    public void SetValue(int value)
    {
        if (value > _max)
        {
            _overflow = value - _max;
            _value = _max;
        }
        else if (value < _max)
        {
            _overflow = 0;
            _value = value;
        }
    }
    public int ApplyValue(int value, out int overflow)
    {
        if (value > _max)
        {
            overflow = value - _max;
            return _max;
        }
        overflow = 0;
        return value;
    }
    public void SetOverflowMax(int max)
        => this._overflowMax = max;
    public int Deficit => this._max - this._value;
    static ProgressFloat CreateCooldown() => new(0, Ticks.PerGameMinute, Ticks.PerGameMinute);
    public ResourceThreshold CurrentThreshold => this.ResourceDef.Worker.GetCurrentThreshold(this);
    public ProgressFloat RechargingDelay = CreateCooldown();
    public float Percentage { get => (float)this.Value / this.Max; set => this.Value = (int)(this.Max * value); }
    public int Min => 0;
    public string Name => this.ResourceDef.Name;
    public ResourceDef Def => this.ResourceDef;
    public ResourceRuntime(ResourceDef def)
    {
        this.ResourceDef = def;
        this.Max = def.BaseMax;
        this.Value = this.Max;
    }
    public ResourceRuntime Bind(Entity entity)
    {
        this.Owner = entity;
        this.Max = this.Def.Worker.GetMax(entity);
        return this;
    }
    public void Tick()
    {

        if (this.RechargingDelay.Value < this.RechargingDelay.Max)
        {
            this.RechargingDelay.Value++;
            return;
        }
        this.ResourceDef.Worker.Tick(this);
    }
    public void ApplyDelta(int delta)
    {
        this.ResourceDef.Worker.ApplyDelta(this, delta);
        if (delta < 0)
            this.RechargingDelay.Value = 0;
    }
    public void ApplyAccumulatorRateDelta(float delta)
    {
        this.Accumulator.ApplyRateDelta(delta);
    }
    public void ApplyAccumulatorDelta(float delta)
    {
        this.Accumulator.Add(delta);
    }
    public ResourceRuntime Initialize(int max, float initPercentage)
    {
        this.Max = max;
        this.Value = (int)(max * initPercentage);
        return this;
    }
    internal void OnNameplateCreated(GameObject parent, Nameplate plate)
    {
        this.ResourceDef.Worker.OnHealthBarCreated(parent, plate, this);
    }
    internal void OnHealthBarCreated(GameObject parent, Nameplate plate)
    {
        this.ResourceDef.Worker.OnHealthBarCreated(parent, plate, this);
    }
    internal Control GetControlBar() => this.ResourceDef.Worker.GetControlBar(this);
    internal Control GetControlLabel() => this.ResourceDef.Worker.GetControlLabel(this);
    public override string ToString()
    {
        return $"{this.ResourceDef.Name}: {this.Value.ToString(this.ResourceDef.Format)} / {this.Max.ToString(this.ResourceDef.Format)}";
    }
    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, this.ResourceDef.Name);
        tag.SaveDef("Def", this.Def);
        tag.Add(this.Value.Save("Value"));
        tag.Add(this.Max.Save("Max"));
        tag.Save("Accumulator", this.Accumulator);
        return tag;
    }
    public static ResourceRuntime Create(SaveTag tag)
    {
        var def = tag.LoadDef<ResourceDef>("Def");
        var resource = new ResourceRuntime(def);
        resource._value = tag.LoadInt("Value");
        resource._max = tag.LoadInt("Max");
        if (tag.TryLoadNew<AccumulatorWithRate>("Accumulator", out var acc))
            resource.Accumulator = acc;
        return resource;
    }
    public static ResourceRuntime Create(IDataReader r) => new ResourceRuntime().Read(r);
    public void Write(IDataWriter w)
    {
        w.Write(this.ResourceDef);
        w.Write(this._value);
        w.Write(this._max);
        w.Write(this.TicksPerRecoverOne);
        w.Write(this.TicksPerDrainOne);
        w.Write(this.Accumulator);
    }
    public ResourceRuntime Read(IDataReader r)
    {
        this.ResourceDef = r.ReadDef<ResourceDef>();
        this._value = r.ReadInt32();
        this._max = r.ReadInt32();
        this.TicksPerRecoverOne = r.ReadInt32();
        this.TicksPerDrainOne = r.ReadInt32();
        this.Accumulator = r.Read<AccumulatorWithRate>();
        return this;
    }
    internal void AddModifier(ResourceRateModifier resourceModifier)
    {
        if (this.Modifiers.Any(m => m.Def == resourceModifier.Def))
            throw new Exception();
        this.Modifiers.Add(resourceModifier);
    }
    public float GetThresholdValue(int index)
    {
        return this.ResourceDef.Worker.GetThresholdValue(this, index);
    }

    Action _unsub = () => { };
    internal void OnDespawn(Entity parent)
    {
        this._unsub();
    }
    internal void Resolve(Entity parent)
    {
        foreach (var (eventType, handler) in this.ResourceDef.Worker.GetEventHandlers())
            _unsub += parent.Map?.Events.ListenTo(eventType, handler);
    }
    public void SetTicksPerRecoverOne(int value)
    {
        this.TicksPerRecoverOne = value;
    }
}

