using System.Collections.Generic;
using Project1.Core.Interactions;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Entities.Actors;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.Towns.AI.Behaviors
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
