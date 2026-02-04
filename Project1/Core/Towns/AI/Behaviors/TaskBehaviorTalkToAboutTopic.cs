using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Project1.Core.Interactions;
using Start_a_Town_;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Framework.Entities.Actors;

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
