using Project1.Core.Entities;
using Project1.Core.UI;
using Project1.Framework.Events;
using Project1.Framework.UI;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;

namespace Project1.Core.Inventory
{
    internal sealed class InventoryListGui : GroupBox
    {
        Table<Entity> TableContents;
        InventoryList Inventory;
        public InventoryListGui(InventoryList inv)
        {
            this.Inventory = inv;
            inv.Notifier.Subscribe(() => this.Invalidate(true));
            inv.ItemAdded += this.Inv_ItemAdded;
            inv.ItemRemoved += this.Inv_ItemRemoved;
            //this.TableContents.ClearControls();

            this.TableContents = new Table<Entity>()
                    .AddColumn("name", 96, o => new Label(() => o.Name, () => Inspector.Refresh(o)) { TooltipFunc = o.GetInventoryTooltip })
                    .AddColumn("weight", 32, o => new Label(() => o.TotalWeight.ToString("0.# kg")));
            this.TableContents.AddItems(inv.Items);
            this.Controls.Add(this.TableContents);

        }

        //public void Refresh(InventoryList inv)
        //{


        //}

        protected override void OnHidden()
        {
            this.Inventory.ItemAdded -= this.Inv_ItemAdded;
            this.Inventory.ItemRemoved -= this.Inv_ItemRemoved;
            base.OnHidden();
        }

        private void Inv_ItemRemoved(Entity obj)
            => this.TableContents.RemoveItem(obj);

        private void Inv_ItemAdded(Entity obj)
            => this.TableContents.AddItem(obj);
    }
    public sealed class InventoryList// : ObservableCollection<Entity>
    {
        readonly internal ChangeNotifier Notifier = new();
        public event Action<Entity> ItemAdded, ItemRemoved;
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
            this.ItemAdded?.Invoke(item);
            item.ContainerNew = this;
            return new(merged, true);
        }

        public void Remove(Entity item)
        {
            this.items.Remove(item);
            item.Container = null;
            this.ItemRemoved?.Invoke(item);
        }
    }
}
