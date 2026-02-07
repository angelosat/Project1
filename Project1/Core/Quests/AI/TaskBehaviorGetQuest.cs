using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Helpers;
using Project1.Core.AI.Behaviors.NodeTypes;
using System.Collections.Generic;

namespace Project1.Core.Quests.AI
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
