using System.Collections.Generic;
using Project1.Core.Interactions;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.AI.Behaviors.Observe
{
    class BehaviorTaskObserveNew : BehaviorExecutePlan
    {
        public override string Name
        {
            get
            {
                return "Observing";
            }
        }
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(this.Plan.TargetA);
            yield return new BehaviorResolveInteraction(this.Plan.TargetA, new InteractionObserve());
        }
    }
}
