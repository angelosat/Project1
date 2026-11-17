using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.ItemRoles;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.AI;

//internal record struct ItemPrefEval(EntityRefId Item, (ItemRoleKey Key, int Score)[] Scores)
//{
//    public readonly int MaxScore => this.Scores.Length > 0 ? this.Scores.Max(r => r.Score) : 0;
//    public readonly int SumScore => this.Scores.Length > 0 ? this.Scores.Sum(r => r.Score) : 0;
//}

public record struct ItemEvaluation(EntityRefId Item, (ItemRoleDef Role, int Score)[] Roles) 
{
    public readonly int MaxScore => this.Roles.Length > 0 ? this.Roles.Max(r => r.Score) : 0;
    public readonly int SumScore => this.Roles.Length > 0 ? this.Roles.Sum(r => r.Score) : 0;
}

internal sealed class Knowledge : Inspectable
{
    internal readonly Dictionary<EntityRefId, ItemEvaluation> KnowledgeItems = [];

    internal void Register(Entity item, ItemEvaluation evaluation)
        => this.KnowledgeItems.Add(item.RefId, evaluation);

    internal ItemEvaluation Query(Entity item)
        => this.KnowledgeItems.TryGetValue(item.RefId, out var entry) ? entry : default;

    internal bool TryQuery(Entity item, out ItemEvaluation entry)
        => this.KnowledgeItems.TryGetValue(item.RefId, out entry);
}

internal sealed class KnowledgeManager(Actor actor)
{
    Actor Actor = actor;
    internal void OnAttachedToWorld()
    {
        var world = this.Actor.World;
        world.Events.ListenTo<EntityDisposedEvent>(OnEntityDisposed);
    }

    private void OnEntityDisposed(EntityDisposedEvent e)
    {
        var knowledge = this.Actor.AI.State.Knowledge;
        knowledge.KnowledgeItems.Remove(e.Entity.RefId);
    }
}