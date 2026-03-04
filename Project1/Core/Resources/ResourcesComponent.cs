using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Materials;
using Project1.Core.UI.NamePlates;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System.Collections.Generic;

namespace Project1.Core.Resources
{
    public sealed class ResourcesComponent : EntityComp
    {
        public override EntityCompDef CompDef => EntityCompDefOf.Resources;
        readonly Dictionary<ResourceDef, Resource> Resources = [];
        public override string Name { get; } = "Resources";

        internal override void CopyFrom(EntityComp comp)
        {
         
            var source = (ResourcesComponent)comp;
            
            foreach (var r in source.Resources.Values)
                this.Add(r.Def);
        }
        public void Add(ResourceDef def)
        {
            this.Resources[def] = new(def) { Owner = this.Owner as Entity };
        }
        public ResourcesComponent()
        {
        }
        public ResourcesComponent(params ResourceDef[] defs)
        {
            throw new System.Exception();
        }
        
        public override void Tick()
        {
            foreach (var item in this.Resources.Values)
                item.Tick();
        }
        public override void OnNameplateCreated(GameObject parent, Nameplate plate)
        {
            foreach (var res in this.Resources.Values)
                res.OnNameplateCreated(parent, plate);
        }
        public override void OnHealthBarCreated(GameObject parent, Nameplate plate)
        {
            foreach (var res in this.Resources.Values)
                res.OnHealthBarCreated(parent, plate);
        }
        public override string ToString()
        {
            string text = "";
            foreach (var item in this.Resources)
                text += item.ToString() + "\n";
            return text.TrimEnd('\n');
        }

        internal override void SaveExtra(SaveTag tag)
        {
            tag.SaveDefWrappers("Resources", this.Resources);
        }
        internal override void Load(GameObject parent, SaveTag tag)
        {
            tag.LoadDefWrappers("Resources", this.Resources);
            this.Resolve();
        }
        public override void Write(IDataWriter writer)
        {
            writer.WriteValues(this.Resources);
        }
        public override void Read(IDataReader reader)
        {
            reader.ReadDefWrappers(this.Resources);
            this.Resolve();
        }
       

        //[InspectorHidden]
        //public Resource this[ResourceDef def] => this.GetResource(def);

        GroupBox _cachedGui;
        GroupBox CachedGui
        {
            get
            {
                if (this._cachedGui is null)
                {
                    this._cachedGui = new GroupBox();
                    foreach(var r in this.Resources.Values)
                        this._cachedGui.AddControlsBottomLeft(r.GetControlBar());
                }
                return this._cachedGui;
            }
        }
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(this.CachedGui);
        }
        internal override IEnumerable<Control> GetSelectionInfo()
        {
            //info.AddInfo(this.CachedGui);
            yield return this.CachedGui;
        }
        internal void AddModifier(ResourceRateModifier resourceRateModifier)
        {
            var resource = this.GetResource(resourceRateModifier.Def.Source);
            resource.AddModifier(resourceRateModifier);
        }
        internal override void ApplyMaterials(Entity parent, Dictionary<string, MaterialDef> materials)
        {
            foreach(var r in this.Resources.Values)
                r.InitMaterials(parent, materials);
        }
        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            foreach (var r in this.Resources.Values)
                tooltip.AddControlsBottomLeft(r.GetControlLabel());
        }
        internal override void Resolve()
        {
            foreach (var r in this.Resources.Values)
            {
                r.Owner = this.Owner;
            }
        }
        internal void ApplyDelta(ResourceDef def, float delta)
        {
            var res = this.Resources[def];
            res.ApplyDelta(delta);
            this.Owner.World?.Events.Post(new ResourceModifiedEvent(this.Owner, def, delta));
        }
        public void SetMax(ResourceDef def, float max)
            => this.Resources[def].Max = max;
        public void SetPercentage(ResourceDef def, float percentage)
           => this.Resources[def].Percentage = percentage;
        public void SetValue(ResourceDef def, float value)
            => this.Resources[def].SetValue(value);
        public void SetTicksPerRecoverOne(ResourceDef def, int value)
            => this.Resources[def].SetTicksPerRecoverOne(value);
        internal Resource GetResource(ResourceDef def)
           => this.Resources[def];
        internal float GetPercentage(ResourceDef def)
            => this.Resources[def].Percentage;
        public EntityResourceView View(ResourceDef def)
            => this.Resources.TryGetValue(def, out var res) ? new(this, res) : null;

        public new class Spec : Spec<ResourcesComponent> 
        {
            public ResourceDef[] Defs;
            public Spec(ResourceDef[] defs)
            {
                this.Defs = defs;
            }
            protected override void ApplyDefaultsTo(ResourcesComponent comp)
            {
                foreach (var def in this.Defs)
                    comp.Add(def);
            }
        }

        public record class EntityResourceView(ResourcesComponent Comp, Resource Resource) : IResourceView
        {
            public ResourceDef Def => this.Resource.Def;
            public float Value
            {
                get => this.Resource.Value;
                set => this.Comp.SetValue(this.Def, value);
            }
            public float Percentage
            {
                get => this.Resource.Value;
                set => this.Comp.SetPercentage(this.Def, value);
            }
            public float Max
            { 
                get => this.Resource.Max;
                set => this.Comp.SetMax(this.Def, value);
            }
            public int TicksPerRecoverOne
            {
                get => this.Resource.TicksPerRecoverOne;
                set => this.Comp.SetTicksPerRecoverOne(this.Def, value);
            }
            public ResourceThreshold CurrentThreshold => this.Resource.CurrentThreshold;

            public void ApplyDelta(float delta) => this.Comp.ApplyDelta(this.Def, delta);
            public float GetThresholdValue(int index) => this.Resource.GetThresholdValue(index);
        }
    }
    interface IResourceView
    {
        public ResourceDef Def { get; }
        float Value { get; set; }
        float Percentage { get; set; }
        float Max { get; set; }
        int TicksPerRecoverOne { get; set; }
        ResourceThreshold CurrentThreshold { get; }
        void ApplyDelta(float delta);
        float GetThresholdValue(int index);
    }
}
