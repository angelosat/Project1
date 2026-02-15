using Project1.Core.AI.Behaviors.NodeTypes;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors.Eating
{
    internal class BehaviorEating : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolveInteraction();
        }
    }
}
