using Start_a_Town_.Framework.AI.NodeTypes;
using System.Collections.Generic;

namespace Start_a_Town_
{
    class TaskBehaviorGetQuest : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var task = this.Plan;
            var actor = this.Actor;
            var qgiver = TargetIndex.A;
            var quest = task.Quest;
            yield return BehaviorHelper.MoveTo(qgiver);
            yield return new BehaviorResolveInteraction(qgiver, () => new InteractionGetQuest(quest));
        }
        public override void CleanUp()
        {
            var actor = this.Actor;
            var task = this.Plan;
            actor.Town.QuestManager.RemoveQuestReceiver(task.Quest);
        }
    }
}
