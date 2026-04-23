using Project1.Core.Entities;
using Project1.Core.Systems.ItemRoles;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.AI;

internal record struct ItemPrefEval(EntityRefId Item, (ItemRoleKey Key, int Score)[] Scores)
{
    public readonly int MaxScore => this.Scores.Length > 0 ? this.Scores.Max(r => r.Score) : 0;
    public readonly int SumScore => this.Scores.Length > 0 ? this.Scores.Sum(r => r.Score) : 0;
}
public record struct ItemEvaluation(EntityRefId Item, (ItemRoleDef Role, int Score)[] Roles) 
{
    public readonly int MaxScore => this.Roles.Length > 0 ? this.Roles.Max(r => r.Score) : 0;
    public readonly int SumScore => this.Roles.Length > 0 ? this.Roles.Sum(r => r.Score) : 0;
}

public sealed class Knowledge : Inspectable
{
    readonly Dictionary<Entity, ItemEvaluation> KnowledgeItems = [];
    public void Register(Entity item, ItemEvaluation evaluation)
        => this.KnowledgeItems.Add(item, evaluation);

    public ItemEvaluation Query(Entity item)
        => this.KnowledgeItems.TryGetValue(item, out var entry) ? entry : default;
    public bool TryQuery(Entity item, out ItemEvaluation entry)
        => this.KnowledgeItems.TryGetValue(item, out entry);
}
