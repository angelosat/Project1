using System.Collections.Generic;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core
{
    class TaskBehaviorStoreInInventory : BehaviorExecutePlan
    {
        public override string Name { get; } = "Storing item in inventory";

        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolveInteraction();
        }
    }
}
