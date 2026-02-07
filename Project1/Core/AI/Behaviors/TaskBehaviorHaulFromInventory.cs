using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.NodeTypes;
using System.Collections.Generic;

namespace Project1.Core
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
