using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Project1.Core.Interactions;
using Project1.Core.AI.Behaviors.Pathing;

namespace Start_a_Town_.AI
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
