using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Materials;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Interfaces;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Resources
{
    public sealed class Resource : MetricWrapper, IProgressBar, ISaveableNewNew<Resource>, IDefWrapper<ResourceDef>, ISerializableNew<Resource>, INamed
    {
        public ResourceDef ResourceDef;
        public List<ResourceRateModifier> Modifiers = new();
        public int TicksPerRecoverOne, TicksPerDrainOne;
        float _max;
        public Resource()
        {
            
        }
        public float Max
        {
            get => this._max; set
            {
                var oldmax = this._max;
                this._max = value;
                this.Value = Math.Min(this.Value, this._max);
            }
        }
        float _value;
        public float Value
        {
            get => this._value;
            set => this._value = Math.Max(0, Math.Min(value, this.Max));
        }
        static Progress CreateCooldown() => new(0, Ticks.PerGameMinute, Ticks.PerGameMinute);
        public ResourceThreshold CurrentThreshold => this.ResourceDef.Worker.GetCurrentThreshold(this);
        public Progress RechargingDelay = CreateCooldown();
        public float Percentage { get => this.Value / this.Max; set => this.Value = this.Max * value; }
        public float Min => 0;
        public string Name => this.ResourceDef.Name;
        public ResourceDef Def => this.ResourceDef;
        public Resource(ResourceDef def)
        {
            this.ResourceDef = def;
            this.Max = def.BaseMax;
            this.Value = this.Max;
        }
        public override void Tick()
        {
            if(this.RechargingDelay.Value < this.RechargingDelay.Max)
            {
                this.RechargingDelay.Value++;
                return;
            }
            this.ResourceDef.Worker.Tick(this);
        }
        public void ApplyDelta(float delta)
        {
            this.ResourceDef.Worker.ApplyDelta(this, delta);
            if (delta < 0)
                this.RechargingDelay.Value = 0;
            //this.Owner.World.Events.Post(new ResourceModifiedEvent(this.Owner, this.Def, this.Value));
            this.Owner.World.Events.Post(new ResourceModifiedEvent(this.Owner, this.Def, delta));
        }
        public Resource Initialize(float max, float initPercentage)
        {
            this.Max = max;
            this.Value = max * initPercentage;
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
            return tag;
        }
        public static Resource Create(SaveTag tag)
        {
            var def = tag.LoadDef<ResourceDef>("Def");
            var resource = new Resource(def);
            tag.TryGetTagValueOrDefault("Value", out resource._value);
            tag.TryGetTagValueOrDefault("Max", out resource._max);
            return resource;
        }
        public static Resource Create(IDataReader r) => new Resource().Read(r);
        public void Write(IDataWriter w)
        {
            w.Write(this.ResourceDef);
            w.Write(this._value);
            w.Write(this._max);
        }
        public Resource Read(IDataReader r)
        {
            this.ResourceDef = r.ReadDef<ResourceDef>();
            this._value = r.ReadSingle();
            this._max = r.ReadSingle();
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
        internal void InitMaterials(Entity obj, Dictionary<string, MaterialDef> materials)
        {
            this.ResourceDef.Worker.InitMaterials(obj, materials);
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
        internal void SetValue(float value)
        {
            this.Value = value;
        }
    }
}
