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

        Dictionary<ResourceDef, Resource> Resources = [];

        public BlockResourcesComp(ResourceDef[] resources)
        {
            foreach (var rDef in resources)
                this.Resources.Add(rDef, new Resource(rDef));
        }
        internal override void GetSelectionInfo(Control container)
        {
            foreach (var r in this.Resources)
                container.AddControls(r.Value.GetControlBar());
        }
    }
}
