using System.Collections.Generic;
using System.Linq;
using Project1.Core.UI;
using Microsoft.Xna.Framework;
using System;
using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.Rendering;
using Project1.Core.Materials;
using Project1.Core.Legacy;
using Project1.Core.Entities;
using Project1.Framework.UI;
using Project1.Framework.Helpers;

namespace Project1.Core.Resources
{
    public abstract class ResourceWorker : MetricWorker
    {
        protected ResourceDef ResourceDef;
        static public Progress Recovery { get { return new Progress(0, Ticks.PerSecond, Ticks.PerSecond); } }
        public ResourceWorker(ResourceDef resourceDef)
        {
            this.ResourceDef = resourceDef;

        }
        public virtual IEnumerable<(Type eventType, Action<IEventPayload> handler)> GetInterests()
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
            return 0;
        }
        protected virtual void OnDepleted(Resource res) { }
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

        public virtual Control GetControlBar(Resource resource)
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
        public virtual Control GetControlLabel(Resource resource)
        {
            return new LabelNew(() => $"{resource.Def.LabelReadable}: {resource.Value} / {resource.Max}");
        }
        public abstract string Description { get; }

        public virtual void Modify(Resource resource, float addValue)
        {
            resource.Value += addValue;
            if (resource.Value <= 0)
                this.OnDepleted(resource);
        }

        public readonly float BaseMax = 100;
        public sealed override void Tick(MetricWrapper wrapper)
        {
            var resource = (Resource)wrapper;
            foreach (var ratemod in resource.Modifiers)
                this.Modify(resource, ratemod.Def.GetRateMod(resource.Owner));
            this.TickExtra(resource);
            this.Modify(resource, this.GetRegenRate(resource));
        }
        protected virtual void updateRec(Resource resource) { }
        protected virtual void TickExtra(Resource resource) { }
        protected virtual float GetRegenRate(Resource resource) => 0;
        public virtual string Format => "";

        public virtual void OnHealthBarCreated(GameObject parent, Nameplate plate, Resource values) { }
        public virtual void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent) { }

        internal virtual void InitMaterials(Entity obj, Dictionary<string, MaterialDef> materials) { }

    }
}