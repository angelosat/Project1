using Project1.Core.AI;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework.Helpers;
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
    internal List<(Entity item, int score, int price)> PotentialImpulseBuysList = [];
    readonly Dictionary<Entity, ItemEvaluation> AllResultsPerItem = [];
    readonly Dictionary<Entity, ProgressInt> Interest = [];

    internal IOrderedEnumerable<(Entity item, int score, int price)> GetResultsSorted() => this.ResultsUnsorted.OrderByDescending(v => this.Interest[v.item].Value);
    internal IEnumerable<(Entity item, int score, int price)> GetResultsImpulse() => this.PotentialImpulseBuysList;
    internal bool HasFinished => this.IndicesRemaining.Count == 0;

    public bool HasResults => this.ResultsUnsorted.Count + this.PotentialImpulseBuysList.Count > 0;

    public void Add(Entity item)
    {
        this.ItemsToBrowse.Add(item);
        this.IndicesRemaining.Enqueue(this.ItemsToBrowse.Count - 1);
    }
    internal void Add(Entity item, ItemEvaluation evaluation)
    {
        this.ItemsToBrowse.Add(item); 
        this.IndicesRemaining.Enqueue(this.ItemsToBrowse.Count - 1);
        this.Interest.Add(item, new ProgressInt(evaluation.MaxScore * Ticks.PerGameMinute));
        this.AllResultsPerItem.Add(item, evaluation);
    }

    public Entity? Dequeue()
        => this.IndicesRemaining.Count > 0 ? this.ItemsToBrowse[this.IndicesRemaining.Dequeue()] : null;

    internal void Register(Entity item, int score, bool isImpulse = false)
    {
        this.Results[score] = item;
        var entry = (item, score, item.GetValueTotal());
        this.ResultsUnsorted.Add(entry);
        if (isImpulse)
            this.PotentialImpulseBuysList.Add(entry);
    }
    internal void AddInterest(Entity item, int delta)
        => this.Interest[item].ApplyDelta(delta);
    internal float GetInterestPercentage(Entity item)
        => this.Interest[item].Percentage;
    internal int GetInterest(Entity item)
        => this.Interest[item].Value;
    const int rampup = 500;
    internal ItemEvaluation GetCachedResult(Entity item)
        => this.AllResultsPerItem[item];
}
