using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Towns.Storage;
using Project1.Core.UI;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Inventory
{
    internal sealed class InventoryListGui : SelectionBoundControl
    {
        readonly Table<Entity> TableContents;
        readonly InventoryList Inventory;
        BlockInventoryComp Comp;
        public InventoryListGui()
        {
            this.TableContents = new Table<Entity>()
                    .AddColumn("name", 96, o => new Label(() => o.Name, () => Inspector.Refresh(o)) { TooltipFunc = o.GetInventoryTooltip })
                    .AddColumn("weight", 48, o => new Label(() => o.TotalWeight.ToString("0.# kg")));
    
            this.Controls.Add(this.TableContents
                .ToScrollableBox(this.TableContents.RowWidth, 16 * (LabelNew.DefaultHeight + 1), ScrollModes.Vertical)
                .ToPanelLabeled("Inventory"));
        }
        
        protected override void OnHidden()
        {
            this.Comp.ItemAdded -= this.Inv_ItemAdded;
            this.Comp.ItemRemoved -= this.Inv_ItemRemoved;
            base.OnHidden();
        }

        private void Inv_ItemRemoved(Entity obj)
            => this.TableContents.RemoveItem(obj);

        private void Inv_ItemAdded(Entity obj)
            => this.TableContents.AddItem(obj);

        protected internal override void OnBind(ISelectable selectable)
        {
            if (selectable is not BlockEntity be)
                return;
            if (!be.TryGetComp<BlockInventoryComp>(out var comp))
                return;
            comp.ItemAdded += this.Inv_ItemAdded;
            comp.ItemRemoved += this.Inv_ItemRemoved;
            this.TableContents.AddItems(comp.Items);
            this.Comp = comp;
        }
    }
    public sealed class InventoryList
    {
        readonly internal ChangeNotifier Notifier = new();
        private readonly List<Entity> items = [];

        public IReadOnlyList<Entity> Items => this.items;
        public readonly struct InsertResult(ISet<Entity> merged, bool added)
        {
            readonly Entity[] _merged = [.. merged];
            readonly public bool Inserted = added;
            public IReadOnlyList<Entity> Merged => this._merged;
            public bool AnyMerge => this._merged.Length > 0;
        }
        public InsertResult Insert(Entity item)
        {
            if (item.ContainerNew == this)
                throw new InvalidOperationException("Item already in this container");
            HashSet<Entity> merged = [];
            foreach (var existing in items)
            {
                if (!existing.CanAbsorb(item))
                    continue;

                var toTake = Math.Min(existing.StackAvailableSpace, item.StackSize);

                existing.Add(toTake);
                item.Consume(toTake);
                merged.Add(existing);
                if (item.StackSize == 0)
                    return new(merged, false);
            }
            item.Detach();
            this.items.Add(item);
            this.Notifier.Notify();
            item.ContainerNew = this;
            return new(merged, true);
        }

        public void Remove(Entity item)
        {
            this.items.Remove(item);
            item.Container = null;
        }
    }
}
