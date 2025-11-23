using System.Collections.Generic;

namespace Start_a_Town_
{
    class TaskBehaviorGetQuest : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var task = this.Task;
            var actor = this.Actor;
            var qgiver = TargetIndex.A;
            var quest = task.Quest;
            yield return BehaviorHelper.MoveTo(qgiver);
            yield return new BehaviorInteractionNew(qgiver, () => new InteractionGetQuest(quest));
        }
        public override void CleanUp()
        {
            var actor = this.Actor;
            var task = this.Task;
            actor.Town.QuestManager.RemoveQuestReceiver(task.Quest);
        }
    }
}
