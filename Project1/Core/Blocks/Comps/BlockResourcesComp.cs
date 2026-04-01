using Project1.Core.Resources;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using static Project1.Core.Blocks.Block;

namespace Project1.Core.Blocks.Comps;

internal class BlockResourcesComp : BlockComp
{
    internal new class Spec(ResourceDef[] Resources) : BlockComp.Spec
    {
        public override Type CompType => typeof(BlockResourcesComp);
        public override BlockComp CreateComp() => new BlockResourcesComp(Resources);
    }
    public override BlockCompDef CompDef => BlockCompDefOf.Resources;

    readonly Dictionary<ResourceDef, Resource> _resources = [];

    public IOrderedEnumerable<Resource> OrderedByDeficit
        => this._resources.Values.OrderByDescending(r => r.Deficit);

    public void ApplyDelta(ResourceDef resource, float delta)
    {
        this._resources[resource].ApplyDelta(delta);
        this.Map.World.Events.Post(new BlockResourceDeltaAppliedEvent(this.Parent.Map, this.Parent.OriginGlobal, resource, delta));
    }

    public bool HasResource(ResourceDef resource)
        => this._resources.ContainsKey(resource);

    public float GetOverflow(ResourceDef resource)
        => this._resources[resource].Overflow;

    public bool TryApplyDelta(ResourceDef resource, float delta)
    {
        if (!this._resources.TryGetValue(resource, out var resourceRuntime))
            return false;
        resourceRuntime.ApplyDelta(delta);
        this.Map.World.Events.Post(new BlockResourceDeltaAppliedEvent(this.Parent.Map, this.Parent.OriginGlobal, resource, delta));
        return true;
    }

    public float GetPercentage(ResourceDef resource)
        => this._resources[resource].Percentage;

    public float GetValue(ResourceDef resource)
        => this._resources[resource].Value;
    public void SetValue(ResourceDef resource, float value)
    {
        this._resources[resource].Value = value;
        this.Map?.World.Events.Post(new BlockResourceValueSetEvent(this.Parent.Map, this.Parent.OriginGlobal, resource, value));
    }
    public void SetToMax(ResourceDef resource)
    {
        var r = this._resources[resource];
        this.SetValue(resource, r.Max);
    }
    public float GetMax(ResourceDef resource)
       => this._resources[resource].Max;
    public void SetMax(ResourceDef resource, float value)
       => this._resources[resource].Max = value;

    public float GetDeficit(ResourceDef resource)
        => this._resources[resource].Deficit;

    public float GetValueOrDefault(ResourceDef resource, float dflt = 0)
        => this._resources.TryGetValue(resource, out var res) ? res.Value : dflt;

    public void SetOverflowMax(ResourceDef resource, float max)
        => this._resources[resource].SetOverflowMax(max);       

    public BlockResourcesComp(ResourceDef[] resources)
    {
        foreach (var rDef in resources)
            this._resources.Add(rDef, new Resource(rDef));
    }
   
    internal override IEnumerable<Control> GetInspectorControls()
    {
        foreach (var r in this._resources)
            yield return r.Value.GetControlBar();
    }
}
