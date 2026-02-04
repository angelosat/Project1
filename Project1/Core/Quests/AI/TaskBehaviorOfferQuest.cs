using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Start_a_Town_;
using Project1.Core.AI.Behaviors.Pathing;

namespace Project1.Core.Quests.AI
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
