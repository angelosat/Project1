using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Core.Systems;
using Project1.Core.Systems.Biology;
using Project1.Core.Systems.ItemRoles;
using Project1.Core.Towns.Services.Shops;
using Project1.Core.World.Actions;

namespace Project1.Core.AI.Thought;

internal sealed class ThoughtProcess_ItemEvaluatorVisitor : ThoughtProcess
{
    internal override void TickOffMap(AIState state)
    {
        var actor = state.Owner;
        if (actor.Biology.IsIncapacitated)
            return;
        var world = (StaticWorld)actor.World;

        var managerPreferences = actor.AI.State.ItemPreferences;
        var managerEvaluation = actor.AI.State.ItemEvaluation;

        while (managerEvaluation.Dequeue() is Entity item)
            managerPreferences.TryCommit(item);

        if (world.Actions.HasActiveAction(actor))
            return;
        if (world.Combat.IsEngaged(actor))
            return;
        var result = SmartEquipSystem.EvaluateEquipmentChange(actor);
        if (result.ItemId != EntityRefId.Null)
        {
            world.Actions.Begin(WorldActionDefOf.Equip, new WorldActionData_Equip(actor) { Decision = result });
        }
    }

    internal override void TickOnMap(AIState state)
    {
        var actor = state.Owner;
        if (!actor.IsSpawned)
            return;
        var preferences = state.ItemPreferences;
        var evaluations = state.ItemEvaluation;
        var list = actor.Map.Town.Shops.GetShoppingListEmpty(actor);

        //while (preferences.Dequeue() is Entity nextEntity)
        while (evaluations.Dequeue() is Entity nextEntity)
        {
            if (!nextEntity.IsForSale)
                continue;
            //var tempEvaluation = preferences.EvaluateWithoutRegistering(nextEntity);
            //if (!state.Knowledge.TryQuery(nextEntity, out var tempEvaluation))
            //    tempEvaluation = ItemEvaluationSystem.Evaluate(actor, nextEntity);
            var tempEvaluation = ItemEvaluationSystem.GetOrEvaluate(actor, nextEntity);

            if (tempEvaluation.Roles.Length == 0)
                continue;

            var evaluationSum = tempEvaluation.SumScore;

            // consider the difference of the sums? or the difference of scores of the best role the item fulfils?
            var existingRolesScoreSum = 0;
            foreach (var (Role, Score) in tempEvaluation.Roles)
                existingRolesScoreSum += preferences.GetExistingScore(Role);
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
