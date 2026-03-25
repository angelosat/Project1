using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.AI.Behaviors.ItemEvaluators.ItemRoles;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Project1.Core.Towns.Shops;

sealed class ShoppingList(Actor actor, List<Entity> items)
{
    readonly Actor Actor = actor;
    readonly List<Entity> ItemsToBrowse = items;
    readonly Queue<int> Indices = new(Enumerable.Range(0, items.Count).Shuffle());
    readonly SortedList<int, Entity> Results = [];
    internal IEnumerable<Entity> GetResults() => this.Results.Values;
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

    internal void Register(Entity item, IEnumerable<(ItemRoleDef role, int score)> results)
    {
        var max = results.Max(e => e.score);
        this.Results[max] = item;
    }
    internal void Register(Entity item, int score)
        => this.Results[score] = item;
}
