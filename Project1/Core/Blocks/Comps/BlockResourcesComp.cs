using Project1.Core.Resources;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Blocks.Comps
{
    internal class BlockResourcesComp : BlockComp
    {
        internal new class Spec(ResourceDef[] Resources) : BlockComp.Spec
        {
            public override Type CompType => typeof(BlockResourcesComp);
            public override BlockComp CreateComp() => new BlockResourcesComp(Resources);
        }
        public override BlockCompDef CompDef => BlockCompDefOf.Resources;

        readonly Dictionary<ResourceDef, Resource> _resources = [];
        //public IReadOnlyDictionary<ResourceDef, Resource> Resources => this._resources;

        public void ApplyDelta(ResourceDef resource, float delta)
        {
            this._resources[resource].ApplyDelta(delta);
            this.Map.World.Events.Post(new BlockResourceModifiedEvent(this.Parent.Map, this.Parent.OriginGlobal, resource, delta));
        }

        public bool HasResource(ResourceDef resource)
            => this._resources.ContainsKey(resource);

        public bool TryApplyDelta(ResourceDef resource, float delta)
        {
            if (!this._resources.TryGetValue(resource, out var resourceRuntime))
                return false;
            resourceRuntime.ApplyDelta(delta);
            this.Map.World.Events.Post(new BlockResourceModifiedEvent(this.Parent.Map, this.Parent.OriginGlobal, resource, delta));
            return true;
        }

        public float GetValue(ResourceDef resource)
            => this._resources[resource].Value;

        public float GetValueOrDefault(ResourceDef resource, float dflt = 0)
            => this._resources.TryGetValue(resource, out var res) ? res.Value : dflt;

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
}
