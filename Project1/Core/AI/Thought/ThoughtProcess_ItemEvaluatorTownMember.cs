using Project1.Core.Entities;
using Project1.Core.Towns.Services.Shops;

namespace Project1.Core.AI.Thought;
internal sealed class ThoughProcess_tItemEvaluatorVisitor : ThoughtProcess
{
    internal override void TickOffMap(AIState state)
    {
    }

    internal override void TickOnMap(AIState state)
    {
        var actor = state.Owner;
        if (!actor.IsSpawned)
            return;
        var manager = state.ItemPreferences;
        var list = actor.Map.Town.Shops.GetShoppingListEmpty(actor);

        while (manager.DequeueUnevaluated() is Entity nextEntity)
        {
            if (!nextEntity.IsForSale)
                continue;
            var tempEvaluation = manager.EvaluateWithoutRegistering(nextEntity);


            if (tempEvaluation.Roles.Length == 0)
                continue;

            var evaluationSum = tempEvaluation.SumScore;

            // consider the difference of the sums? or the difference of scores of the best role the item fulfils?
            var existingRolesScoreSum = 0;
            foreach (var (Role, Score) in tempEvaluation.Roles)
                existingRolesScoreSum += manager.GetExistingScore(Role);
            var scoreDiff = evaluationSum - existingRolesScoreSum;
            if (existingRolesScoreSum == 0)
                scoreDiff += 100; // if the item isn't replacing anything, boost its interest
            //list.Add(nextEntity, tempEvaluation);
            list.Add(nextEntity, tempEvaluation, scoreDiff);
        }

        //if(list.GetResultsSorted().FirstOrDefault() is var best && best.item is Entity item)
        //{
        //    var placeholderThreshold = 2;
        //    if(best.score >= placeholderThreshold)
        //    {
        //        list.Impulse = item;
        //    }
        //}
    }
}
internal class ThoughtProcess_ItemEvaluatorTownMember : ThoughtProcess
{
    internal override void TickOffMap(AIState state)
    {
    }

    internal override void TickOnMap(AIState state)
    {
        if (!state.Owner.IsSpawned)
            return;
        var manager = state.ItemPreferences;
        if(manager.DequeueUnevaluated() is Entity item)
        {
            if (item.OwnerId != EntityRefId.Null)
                return;
            if (item.IsForSale)
                return;
            var result = manager.EvaluateAndRegister(item);
            //state.Knowledge.Register(item, result);
            if (state.Owner.Map.Town.IsClaimedBySystem(item))
                return;
            manager.TryPreCommit(item, result);
        }
    }
}
