using System.Collections.Generic;

namespace Start_a_Town_
{
    class TaskBehaviorHaulFromInventory : BehaviorExecutePlan
    {
        public override string Name { get; } = "Hauling From Inventory";

        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolveInteraction();
        }
    }
}
