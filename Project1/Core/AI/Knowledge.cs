using Project1.Core.Entities;
using Project1.Core.Towns.AI.Behaviors.ItemEvaluators.ItemRoles;
using System.Collections.Generic;

namespace Project1.Core.AI;

public record struct ItemEvaluation(EntityRefId Item, (ItemRoleDef Role, int Score)[] Roles) { }
public sealed class Knowledge
{
    readonly Dictionary<Entity, ItemEvaluation> KnowledgeItems = [];
    public void Register(Entity item, ItemEvaluation evaluation)
        => this.KnowledgeItems.Add(item, evaluation);

    public ItemEvaluation Query(Entity item)
        => this.KnowledgeItems.TryGetValue(item, out var entry) ? entry : default;
    public bool TryQuery(Entity item, out ItemEvaluation entry)
        => this.KnowledgeItems.TryGetValue(item, out entry);
}
