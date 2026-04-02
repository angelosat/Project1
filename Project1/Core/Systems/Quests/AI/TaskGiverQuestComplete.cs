using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Towns;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Systems.Quests.AI
{
    class TaskGiverQuestComplete : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            return null;
            var props = actor.GetVisitorProperties();
            var quests = props.GetQuests();
            foreach(var q in quests)
            {
                if (!q.IsCompleted(actor))
                    continue;
                var qgiver = q.Giver;
                actor.Town.QuestManager.HandleQuestReceiver(actor, q);
                return new Plan(typeof(TaskBehaviorQuestComplete), qgiver) { Quest = q.ID };
            }
            return null;
        }
    }
}
