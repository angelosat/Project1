using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Behaviors.Pathing;
using System;
using System.Collections.Generic;

namespace Project1.Core.Towns.AI.Behaviors
{
    class TaskBehaviorTalkToAboutTopic : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(TargetIndex.A);
            throw new NotImplementedException();
            //yield return new BehaviorResolveInteraction(TargetIndex.A, () => new InteractionConversationGradual(this.Actor.GetNextConversationTopicFor(this.Plan.TargetA.Object as Actor)));
        }
    }
}
