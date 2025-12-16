using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI
{
    class BehaviorTaskObserveNew : BehaviorPerformTask
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
            yield return new BehaviorGetAtNewNew(this.Task.TargetA);
            yield return new BehaviorInteractionNew(this.Task.TargetA, new InteractionObserve());
        }
    }
}
