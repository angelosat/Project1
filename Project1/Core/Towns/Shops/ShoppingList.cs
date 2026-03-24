using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Project1.Core.Towns.Shops;

sealed class ShoppingList(Actor actor, List<Entity> items)
{
    readonly Actor Actor = actor;
    readonly List<Entity> ItemsToBrowse = items;
    readonly Queue<int> Indices = new(Enumerable.Range(0, items.Count).Shuffle());
    internal bool HasFinished => this.Indices.Count == 0;

    public void Add(Entity item)
    {
        this.ItemsToBrowse.Add(item);
        this.Indices.Enqueue(this.ItemsToBrowse.Count - 1);
    }
    public Entity? Peek()
        => this.Indices.Count > 0 ? this.ItemsToBrowse[this.Indices.Peek()] : null;
    public Entity? Dequeue()
    => this.Indices.Count > 0 ? this.ItemsToBrowse[this.Indices.Dequeue()] : null;
}
