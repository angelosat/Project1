using System.Collections.Generic;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.Systems.Quests.AI
{
    class TaskBehaviorOfferQuest : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var task = this.Plan;
            var actor = this.Actor;
            var manager = actor.Town.QuestManager;
            yield return new BehaviorStopMoving();
            yield return new BehaviorWait(() => manager.GetNextQuestReceiver(actor) == null);
        }
    }
}
