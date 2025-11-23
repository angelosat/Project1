using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorOfferQuest : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var task = this.Task;
            var actor = this.Actor;
            var manager = actor.Town.QuestManager;
            yield return new BehaviorStopMoving();
            yield return new BehaviorWait(() => manager.GetNextQuestReceiver(actor) == null);
        }
    }
}
