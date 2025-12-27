using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorTalkToAboutTopic : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(TargetIndex.A);
            yield return new BehaviorResolveInteraction(TargetIndex.A, () => new InteractionConversationGradual(this.Actor.GetNextConversationTopicFor(this.Plan.TargetA.Object as Actor)));
        }
    }
}
