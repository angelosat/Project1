using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Towns.Storage;
using Project1.Core.UI;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Inventory;

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
            .ToScrollableBox(this.TableContents.RowWidth, 16 * (UIManager.DefaultLabelHeight + 1), ScrollModes.Vertical)
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
public readonly struct InsertResult(ISet<Entity> merged, bool added, int newTotal)
{
    readonly Entity[] _merged = [.. merged];
    readonly public bool Inserted = added;
    public IReadOnlyList<Entity> Merged => this._merged;
    public bool AnyMerge => this._merged.Length > 0;
    public readonly int NewTotal = newTotal;// { get; private set; }
}
public sealed class InventoryList
{
    readonly internal ChangeNotifier Notifier = new();
    private readonly List<Entity> items = [];
    public event Action<Entity> ItemAdded, ItemRemoved;

    public IReadOnlyList<Entity> Items => this.items;
    
    public void InsertInt(Entity item)
    {
        item.Detach();
        this.items.Add(item);
        this.Notifier.Notify();
        item.ContainerNew = this;
        this.ItemAdded?.Invoke(item);
    }

    public InsertResult Insert(Entity item)
    {
        if (item.ContainerNew == this)
            throw new InvalidOperationException("Item already in this container");
        HashSet<Entity> merged = [];
        var mergeCanditates = items.Where(i => i.Matches(item)).ToList();
        var total = mergeCanditates.Sum(i => i.StackSize);
        foreach (var existing in mergeCanditates)
        {
            //if (!existing.CanAbsorb(item))
            //    continue;
            if (existing.IsStackFull)
                continue;

            var toTake = Math.Min(existing.StackAvailableSpace, item.StackSize);

            existing.Add(toTake);
            item.Consume(toTake);
            merged.Add(existing);
            total += toTake;
            if (item.StackSize == 0)
                return new(merged, false, total);
        }
        item.Detach();
        this.items.Add(item);
        this.Notifier.Notify();
        item.ContainerNew = this;
        this.ItemAdded?.Invoke(item);
        total += item.StackSize;
        return new(merged, true, total);
        //if (item.ContainerNew == this)
        //    throw new InvalidOperationException("Item already in this container");
        //HashSet<Entity> merged = [];

        //foreach (var existing in items)
        //{
        //    if (!existing.CanAbsorb(item))
        //        continue;

        //    var toTake = Math.Min(existing.StackAvailableSpace, item.StackSize);

        //    existing.Add(toTake);
        //    item.Consume(toTake);
        //    merged.Add(existing);
        //    if (item.StackSize == 0)
        //        return new(merged, false);
        //}
        //item.Detach();
        //this.items.Add(item);
        //this.Notifier.Notify();
        //item.ContainerNew = this;
        //this.ItemAdded?.Invoke(item);

        //return new(merged, true);
    }

    public void Remove(Entity item)
    {
        this.items.Remove(item);
        item.Container = null;
        this.ItemRemoved?.Invoke(item);
    }

    public bool Contains(Entity item)
        => this.Items.Contains(item);
}
