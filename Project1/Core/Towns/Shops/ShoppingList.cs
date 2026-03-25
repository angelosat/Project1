using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Project1.Core.Towns.Shops;

sealed class ShoppingList(Actor actor, List<Entity> items)
{
    readonly Actor Actor = actor;
    readonly List<Entity> ItemsToBrowse = items;
    readonly Queue<int> IndicesRemaining = new(Enumerable.Range(0, items.Count).Shuffle());
    readonly SortedList<int, Entity> Results = [];
    readonly List<(Entity item, int score, int price)> ResultsUnsorted = [];
    [Obsolete]
    internal Queue<(Entity item, int score, int price)> PotentialImpulseBuys = [];
    internal List<(Entity item, int score, int price)> PotentialImpulseBuysList = [];

    internal IEnumerable<Entity> GetResults() => this.Results.Values;
    internal IOrderedEnumerable<(Entity item, int score, int price)> GetResultsSorted() => this.ResultsUnsorted.OrderByDescending(v => v.score);
    internal IEnumerable<(Entity item, int score, int price)> GetResultsImpulse() => this.PotentialImpulseBuysList;
    [Obsolete]
    internal (Entity item, int score, int price) DequeueImpulse() => this.PotentialImpulseBuys.Count > 0 ? this.PotentialImpulseBuys.Dequeue() : default;
    internal bool HasFinished => this.IndicesRemaining.Count == 0;

    //public bool HasResults => this.ResultsUnsorted.Count + this.PotentialImpulseBuys.Count > 0;
    public bool HasResults => this.ResultsUnsorted.Count + this.PotentialImpulseBuysList.Count > 0;

    public void Add(Entity item)
    {
        this.ItemsToBrowse.Add(item);
        this.IndicesRemaining.Enqueue(this.ItemsToBrowse.Count - 1);
    }
    public Entity? Peek()
        => this.IndicesRemaining.Count > 0 ? this.ItemsToBrowse[this.IndicesRemaining.Peek()] : null;
    public Entity? Dequeue()
        => this.IndicesRemaining.Count > 0 ? this.ItemsToBrowse[this.IndicesRemaining.Dequeue()] : null;

    //internal void Register(Entity item, IEnumerable<(ItemRoleDef role, int score)> results)
    //{
    //    var max = results.Max(e => e.score);
    //    this.Results[max] = item;
    //}
    internal void Register(Entity item, int score, bool isImpulse = false)
    {
        this.Results[score] = item;
        var entry = (item, score, item.GetValueTotal());
        this.ResultsUnsorted.Add(entry);
        if (isImpulse)
        {
            this.PotentialImpulseBuys.Enqueue(entry);
            this.PotentialImpulseBuysList.Add(entry);
        }
    }
}
