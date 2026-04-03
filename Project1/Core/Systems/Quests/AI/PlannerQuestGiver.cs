using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Systems.Quests.AI
{
    class PlannerQuestGiver : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var manager = actor.Map.Town.QuestManager;
            var nextQuestReceiver = manager.GetNextQuestReceiver(actor);
            if (nextQuestReceiver == null)
                return null;
            return new Plan(typeof(TaskBehaviorOfferQuest), nextQuestReceiver);
        }
    }
}
