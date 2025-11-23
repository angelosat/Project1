using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorTalkToAboutTopic : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorGetAtNewNew(TargetIndex.A);
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionConversationGradual(this.Actor.GetNextConversationTopicFor(this.Task.TargetA.Object as Actor)));
        }
    }
}
