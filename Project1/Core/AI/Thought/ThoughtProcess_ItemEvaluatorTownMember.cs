using Project1.Core.Entities;
using Project1.Core.Systems.ItemRoles;
using Project1.Core.Towns.Services.Shops;

namespace Project1.Core.AI.Thought;
internal sealed class ThoughtProcess_ItemEvaluatorTownMember : ThoughtProcess
{
    internal override void TickOffMap(AIState state)
    {
    }

    internal override void TickOnMap(AIState state)
    {
        var actor = state.Owner;
        if (!actor.IsSpawned)
            return;
        //var manager = state.ItemPreferences;
        var preferences = state.ItemPreferences;
        var evaluations = state.ItemEvaluation;
        //if (preferences.Dequeue() is Entity item)
        if (evaluations.Dequeue() is Entity item)
        {
            if (item.Map is null) // item has despawned since being enqueued for evaluation
                return;
            if (item.OwnerId != EntityRefId.Null)
                return;
            if (item.IsForSale)
                return;
            //var result = preferences.EvaluateAndRegister(item);
            //if (!state.Knowledge.TryQuery(item, out var result))
            //{
            //    result = ItemEvaluationSystem.Evaluate(actor, item);
            //    state.Knowledge.Register(item, result);
            //}
            var result = ItemEvaluationSystem.GetOrEvaluateAndRegister(actor, item);
            //state.Knowledge.Register(item, result);
            if (actor.Map.Town.IsClaimedBySystem(item))
                return;
            preferences.TryPreCommit(item, result);
        }
    }
}
