using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Systems.Materials;
using Project1.Core.UI.NamePlates;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Resources;

public abstract class ResourceWorker// : MetricWorker
{
    protected ResourceDef ResourceDef;
    static public ProgressFloat Recovery { get { return new ProgressFloat(0, Ticks.PerSecond, Ticks.PerSecond); } }
    public ResourceWorker(ResourceDef resourceDef)
    {
        this.ResourceDef = resourceDef;
    }
    public virtual IEnumerable<(Type eventType, Action<IEventPayload> handler)> GetEventHandlers()
    {
        yield break;   
    }
    public readonly List<ResourceThreshold> Thresholds = [];
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
    public ResourceThreshold GetCurrentThreshold(float percentage)
    {
        return this.Thresholds.FirstOrDefault(t => percentage <= t.Value);
    }
    public abstract Color GetBarColor(Resource resource);
    public virtual string GetBarLabel(Resource resource)
    {
        return this.GetLabel(resource);
    }
    public virtual string GetBarHoverText(Resource resource)
    {
        return $"{((int)resource.ValueWithOverflow).ToString(this.Format)} / {resource.Max.ToString(this.Format)} ({resource.MaxWithOverflow})";
        return $"{((int)resource.Value).ToString(this.Format)} / {resource.Max.ToString(this.Format)}";
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

    public virtual void ApplyDelta(Resource resource, float delta)
    {
        resource.Value += delta;
        //resource.SetValue(resource.Value + delta);
        if (resource.Value <= 0)
            this.OnDepleted(resource);
    }

    public readonly float BaseMax = 100;
    public /*sealed override*/ void Tick(Resource resource)
    {
        //var resource = (Resource)wrapper;
        foreach (var ratemod in resource.Modifiers)
            this.ApplyDelta(resource, ratemod.Def.GetRateMod(resource.Owner));
        this.TickExtra(resource);
        this.ApplyDelta(resource, this.GetRegenRate(resource));
    }
    protected virtual void updateRec(Resource resource) { }
    protected virtual void TickExtra(Resource resource) { }
    protected virtual float GetRegenRate(Resource resource) => resource.Def.BaseRegenRate;
    public virtual string Format => "";
    public virtual void OnHealthBarCreated(GameObject parent, Nameplate plate, Resource values) { }
    public virtual void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent) { }
    internal virtual void InitMaterials(Entity obj, Dictionary<string, MaterialDef> materials) { }
}