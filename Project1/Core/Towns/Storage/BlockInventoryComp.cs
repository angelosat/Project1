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

        //readonly ContainerList Contents = [];
        readonly InventoryList Contents = new();

        internal override bool TryConsume(Entity item)
        {
            this.Insert(item);
            $"{item.RefId}:{item.Name} deposited inside {this.Parent}".ToConsole();
            return true;
        }
        internal void Insert(Entity item)
        {
            var result = this.Contents.Insert(item);
            if (result.Inserted)
                this.Map.Events.Post(new BlockInventoryItemAddedEvent(this.Parent, item));
        }
        internal void Remove(Entity item)
        {
            this.Contents.Remove(item);
        }
        //internal override IEnumerable<Control> GetInspectorControls()
        //{
        //    yield return this.Contents.Gui;
        //}
        internal override IEnumerable<Control> GetInspectorControls()
        {
            yield return new InventoryListGui(this.Contents);
        }
    }
}
