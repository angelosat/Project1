using Project1.Core.Quests;
using Project1.Framework.Entities.Actors;
using Start_a_Town_;
using Start_a_Town_.Core;

namespace Project1.Core.Quests.AI
{
    class TaskGiverGetQuests : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var town = actor.Map.Town;
            var quests = town.QuestManager.GetQuestDefs();
            foreach(var q in quests)
            {
                // TODO check before receiving quest:
                // reward vs difficulty
                // is completable (has access to areas that the required items can be found)
                var giver = q.Giver;
                if (giver == null)
                    continue;
                if (!q.CanGiveQuestTo(actor))
                    continue; 
                if (!actor.CanAcceptQuest(q))
                    continue;
                if (!actor.CanReach(giver))
                    continue;
                if (!Decide(actor, q))
                    continue;
                actor.Town.QuestManager.HandleQuestReceiver(actor, q);
                return new Plan(QuestTaskDefOf.AcceptQuest, giver) { Quest = q.ID };
            }
            return null;
        }

        private bool Decide(Actor actor, QuestDef q)
        {
            return q.GetRewardRatio() >= 1;
        }
    }
}
