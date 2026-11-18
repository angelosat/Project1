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

    static ItemEvaluation Evaluate(Actor actor, Entity item)
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
    
    public static ItemEvaluation GetOrEvaluate(Actor actor, Entity item)
    {
        var knowledge = actor.AI.State.Knowledge;

        if (knowledge.TryQuery(item, out var evaluation))
            return evaluation;

        return Evaluate(actor, item);
    }

    public static ItemEvaluation GetOrEvaluateAndRegister(Actor actor, Entity item)
    {
        var knowledge = actor.AI.State.Knowledge;

        if (knowledge.TryQuery(item, out var evaluation))
            return evaluation;

        evaluation = Evaluate(actor, item);
        knowledge.Register(item, evaluation);

        return evaluation;
    }
}
