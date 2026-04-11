using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Systems.Materials;
using Project1.Core.UI.NamePlates;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Resources;

public readonly record struct ResourceSnapshot(ResourceDef Def, float Value, float Max) { }
public sealed class ResourcesComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Resources;
    readonly Dictionary<ResourceDef, Resource> Resources = [];
    public override string Name { get; } = "Resources";

    internal override void CopyFrom(EntityComp comp)
    {
        var source = (ResourcesComp)comp;
        
        foreach (var r in source.Resources.Values)
            this.Add(r.Def);
    }
    public void Add(ResourceDef def)
    {
        this.Resources[def] = new(def) { Owner = this.Owner as Entity };
    }
    public ResourcesComp()
    {
    }
    public ResourcesComp(params ResourceDef[] defs)
    {
        throw new System.Exception();
    }
    public ResourceSnapshot Get(ResourceDef def)
    {
        var res = this.Resources[def];
        return new(def, res.Value, res.Max);
    }
    public float? GetValueOrDefault(ResourceDef def)
        => this.Resources.TryGetValue(def, out var val) ? val.Value : null;

    internal float? GetPercentageOrDefault(ResourceDef def)
        => this.Resources.TryGetValue(def, out var val) ? val.Percentage : null;

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
    internal override GroupBox GetDetailedGui()
    {
        var box = new GroupBox();
        foreach (var r in this.Resources.Values)
            box.AddControlsBottomLeft(r.GetControlBar());
        return box;
    }
    //internal override void GetSelectionInfo(IUISelection info, GameObject parent)
    //{
    //    info.AddInfo(this.CachedGui);
    //}
    internal override IEnumerable<Control> GetSelectionInfo()
    {
        //yield return this.CachedGui;
        foreach (var r in this.Resources.Values)
            yield return r.GetControlBar();
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
        // HACK
        if(this.Owner.Profile is ActorDnaDef dna)
        foreach (var res in dna.Resources.Where(r => !this.Resources.ContainsKey(r)))
            this.Add(res);
    }
    internal void ApplyDelta(ResourceDef def, float delta)
    {
        var res = this.Resources[def];
        res.ApplyDelta(delta);
        this.Owner.World?.Events.Post(new ResourceDeltaAppliedEvent(this.Owner, def, delta));
    }
    public float GetMax(ResourceDef def)
        => this.Resources[def].Max;
    public void SetMax(ResourceDef def, float max)
        => this.Resources[def].Max = max;
    public void SetPercentage(ResourceDef def, float percentage)
       => this.Resources[def].Percentage = percentage;
    public void SetValue(ResourceDef def, float value)
        => this.Resources[def].SetValue(value);
    public int GetTicksPerRecoverOne(ResourceDef def)
        => this.Resources[def].TicksPerRecoverOne;
    public void SetTicksPerRecoverOne(ResourceDef def, int value)
        => this.Resources[def].SetTicksPerRecoverOne(value);
    internal Resource GetResource(ResourceDef def)
       => this.Resources[def];
    internal float GetPercentage(ResourceDef def)
        => this.Resources[def].Percentage;


    [Obsolete]
    public EntityResourceViewOld ViewOld(ResourceDef def)
        => this.Resources.TryGetValue(def, out var res) ? new(this, res) : null;
    public EntityResourceView View(ResourceDef def)
        => this.Resources.TryGetValue(def, out _) ? new(this, def) : null;

    internal float GetValue(ResourceDef def)
        => this.Resources[def].Value;

    public new sealed class Spec(ResourceDef[] defs) : Spec<ResourcesComp> 
    {
        public ResourceDef[] Defs = defs;

        protected override void ApplyDefaultsTo(ResourcesComp comp)
        {
            foreach (var def in this.Defs)
                comp.Add(def);
        }
    }
    public ResourceThreshold GetCurrentThreshold(ResourceDef def)
        => this.Resources[def].CurrentThreshold;
    public float GetThresholdValue(ResourceDef def, int index)
        => this.Resources[def].GetThresholdValue(index);
    public record class EntityResourceView(ResourcesComp Comp, ResourceDef Def) : IResourceView
    {
        public float Value
        {
            get => this.Comp.GetValue(this.Def);
            set => this.Comp.SetValue(this.Def, value);
        }
        public float Percentage
        {
            get => this.Comp.GetPercentage(this.Def);
            set => this.Comp.SetPercentage(this.Def, value);
        }
        public float Max
        {
            get => this.Comp.GetMax(this.Def);
            set => this.Comp.SetMax(this.Def, value);
        }
        public int TicksPerRecoverOne
        {
            get => this.Comp.GetTicksPerRecoverOne(this.Def);
            set => this.Comp.SetTicksPerRecoverOne(this.Def, value);
        }

        public void ApplyDelta(float delta) => this.Comp.ApplyDelta(this.Def, delta);
        //public ResourceThreshold CurrentThreshold => this.Resource.CurrentThreshold;
        //public float GetThresholdValue(int index) => this.Resource.GetThresholdValue(index);
        public ResourceThreshold CurrentThreshold => this.Comp.GetCurrentThreshold(this.Def);
        public float GetThresholdValue(int index) => this.Comp.GetThresholdValue(this.Def, index);
    }
    [Obsolete]
    public record class EntityResourceViewOld(ResourcesComp Comp, Resource Resource) : IResourceView
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
