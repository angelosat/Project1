using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Quests;

sealed class PlannerQuest : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        //if (actor.IsHauling)
        //    return null;
        var map = actor.Map;
        var manager = map.Town.QuestManagerNew;
        var boards = manager.QuestBoards;
        if (!TryScan(actor, manager, boards, out var board, out var availableQuests))
            return null;
        //var allquests = manager.AllQuests;
        //foreach(var q in allquests)
        if(manager.GetNextCompletedQuest(actor) is QuestRuntime_Deliver quest)
        {
            if(actor.Hauled is Entity carried && quest.IsFulfilledBy(carried))
                return new Plan(QuestDefOf.PlanQuestComplete, map, board);
            if (actor.Inventory.Where(quest.IsFulfilledBy).FirstOrDefault() is not Entity item)
                throw new System.Exception();
            return new Plan(PlanDefOf.RetrieveFromInventory, item);
        }
        if (actor.IsHauling)
            return null;
        foreach (var q in availableQuests)
        {
            if (!manager.HasQuest(actor, q.Id))
                return new Plan(QuestDefOf.PlanQuest, map, board);
        }
        return null;
    }

    private static bool TryScan(Actor actor, TownComp_Quests manager, IEnumerable<IntVec3> boards, out IntVec3 board, out IEnumerable<QuestRuntime> availableQuests)
    {
        //foundBoard = false;
        board = default;
        availableQuests = null;
        foreach (var b in boards)
        {
            if (!actor.CanReachAndReserve(b))
                continue;
            availableQuests = manager.GetAvailableQuests(b);
            if (!availableQuests.Any())
                continue;
            board = b;
            return true;
        }
        return false;
    }
}
