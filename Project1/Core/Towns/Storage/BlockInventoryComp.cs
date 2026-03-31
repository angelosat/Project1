using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Legacy.Storage;
using Project1.Core.Systems.Inventory;
using Project1.Framework;
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

        readonly InventoryList Contents;
        public IReadOnlyList<Entity> Items => this.Contents.Items;

        public int Priority => (int)StoragePriority.Low;
        public event Action<Entity> ItemAdded, ItemRemoved;

        public BlockInventoryComp()
        {
            this.Contents = new();
            this.Contents.ItemAdded += e => this.ItemAdded?.Invoke(e);
            this.Contents.ItemRemoved += e => this.ItemRemoved?.Invoke(e);
        }
        internal override bool TryConsume(Entity item)
        {
            this.Insert(item);
            $"{item.RefId}:{item.Name} deposited inside {this.Parent}".ToConsole();
            return true;
        }
        internal void InsertInt(Entity item)
        {
            this.Contents.InsertInt(item);
            item.Transform.Anchor = this.Parent;
            this.Map.Events.Post(new BlockInventoryItemAddedEvent(this.Parent, item));
        }
        internal void Insert(Entity item)
        {
            var result = this.Contents.Insert(item);
            if (result.Inserted)
            {
                item.Transform.Anchor = this.Parent;
                //this.ItemAdded?.Invoke(item);
                this.Map.Events.Post(new BlockInventoryItemAddedEvent(this.Parent, item));
            }
        }
        internal void Remove(Entity item)
        {
            this.Contents.Remove(item);
            //this.ItemRemoved?.Invoke(item);
        }
        internal override IEnumerable<(string label, Type type)> GetSelectionTabs()
        {
            yield return ("Storage", typeof(InventoryListGui));
        }

        internal bool Accepts(Entity item)
            => true;

        internal int AvailableCapacityFor(Entity item)
            => item.StackMax;

        internal bool Contains(Entity item)
            => this.Contents.Contains(item);
    }
}
