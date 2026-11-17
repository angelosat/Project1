using Project1.Core.AI;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Systems.ItemRoles;

[EnsureStaticCtorCall]
public static class ItemEvaluationSystem
{
    private static ItemRoleDef[] ItemRoles => field ??= [.. Def.Get<ItemRoleDef>()];

    public static ItemEvaluation Evaluate(Actor actor, Entity item)
    {
        var results = new List<(ItemRoleDef Role, int Score)>();

        foreach (var role in ItemRoles)
        {
            var score = role.Worker.GetInventoryScore(actor, item);

            if (score > 0)
                results.Add((role, score));
        }

        return new ItemEvaluation(item.RefId, [.. results]);
    }
}
