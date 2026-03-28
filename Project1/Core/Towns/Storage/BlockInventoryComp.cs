using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Inventory;
using Project1.Framework;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Towns.Storage
{
    internal sealed class BlockInventoryComp : BlockComp
    {
        public new sealed class Spec : BlockComp.Spec
        {
            public override Type CompType => typeof(BlockInventoryComp);

            public override BlockInventoryComp CreateComp() => new();
        }
        public override BlockCompDef CompDef => BlockCompDefOf.Inventory;

        readonly ContainerList Contents = [];

        internal override bool TryConsume(Entity item)
        {
            this.Contents.Add(item);
            $"{item.RefId}:{item.Name} deposited inside {this.Parent}".ToConsole();
            return true;
        }
        //internal override IEnumerable<Control> GetInspectorControls()
        //{
        //    yield return this.Contents.Gui;
        //}
    }
}
