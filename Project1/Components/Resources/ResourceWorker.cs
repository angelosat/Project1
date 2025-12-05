using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_
{
    public abstract class ResourceWorker
    {
        protected ResourceDef ResourceDef;
        static public Progress Recovery { get { return new Progress(0, Ticks.PerSecond, Ticks.PerSecond); } }
        public ResourceWorker(ResourceDef resourceDef)
        {
            this.ResourceDef = resourceDef;

        }
        public virtual IEnumerable<(Type eventType, Action<EventPayloadBase> handler)> GetInterests()
        {
            yield break;   
        }
        internal virtual void HandleRemoteCall(GameObject parent, ObjectEventArgs e, Resource resource)
        {
        }
        public virtual void SetMaterial(MaterialDef mat) { }

        public readonly List<ResourceThreshold> Thresholds = new();
        public ResourceWorker AddThreshold(string label, float value = 1)
        {
            var t = new ResourceThreshold(label, value);
            this.Thresholds.Add(t);
            this.Thresholds.Sort((a, b) => a.Value.CompareTo(b.Value));
            return this;
        }
       
        public float GetThresholdValue(Resource res, int index)
        {
            return 
                //this.Root?.GetThresholdValue(index) ?? 
                0;
        }

        public string GetLabel(Resource res)
        {
            return this.GetCurrentThreshold(res)?.Label ?? "";
        }
        public ResourceThreshold GetCurrentThreshold(Resource res)
        {
            return this.Thresholds.FirstOrDefault(t => res.Percentage <= t.Value);
        }
        public abstract Color GetBarColor(Resource resource);
        public virtual string GetBarLabel(Resource resource)
        {
            return this.GetLabel(resource);
        }
        public virtual string GetBarHoverText(Resource resource)
        {
            return $"{resource.Value.ToString(this.Format)} / {resource.Max.ToString(this.Format)}";
        }

        public virtual Control GetControl(Resource resource)
        {
            var bar = new Bar()
            {
                Object = resource,
                ColorFunc = () => this.GetBarColor(resource),
                TextFunc = () => this.GetBarLabel(resource),
                HoverFunc = () => this.GetBarHoverText(resource)
            };
            return bar;
        }

        public abstract string Description { get; }

        public virtual void Modify(Resource resource, float addValue)
        {
            resource.Value += addValue;
        }

        public readonly float BaseMax = 100;

        public virtual void Tick(GameObject parent, Resource resource)
        {
            foreach (var ratemod in resource.Modifiers)
                this.Modify(resource, ratemod.Def.GetRateMod(parent));
        }
        public virtual bool HandleMessage(Resource resource, GameObject parent, ObjectEventArgs e = null) { return false; }

        public virtual string Format => "";

        public virtual void OnHealthBarCreated(GameObject parent, UI.Nameplate plate, Resource values) { }
        public virtual void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent) { }

        internal virtual void InitMaterials(Entity obj, Dictionary<string, MaterialDef> materials) { }

    }
}
