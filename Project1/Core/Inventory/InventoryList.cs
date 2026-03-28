using Project1.Core.Entities;
using Project1.Framework.Events;
using System;
using System.Collections.Generic;

namespace Project1.Core.Inventory
{
    public class InventoryList
    {
        public enum InventoryResult { ExistingStacksIncreased, NewItemAdded };
        
        readonly ChangeNotifier Notifier = new();
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
