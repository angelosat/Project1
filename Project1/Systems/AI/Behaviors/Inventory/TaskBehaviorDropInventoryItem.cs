using System.Collections.Generic;

namespace Start_a_Town_
{
    class TaskBehaviorDropInventoryItem : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            // for now, just haul item from inventory and finish behavior, because cleaning up after the behavior, drops the current carried item if it's not an itemrole preference
            yield return new BehaviorResolveInteraction(TargetIndex.A, () => null);// new InteractionHaul());
            yield return new BehaviorResolveInteraction(TargetIndex.A, () => new InteractionThrow());
        }
    }
}
