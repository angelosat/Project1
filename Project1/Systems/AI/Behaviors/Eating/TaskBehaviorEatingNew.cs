using Start_a_Town_.Framework.AI.NodeTypes;
using System.Collections.Generic;

namespace Start_a_Town_
{
    internal class TaskBehaviorEatingNew : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolveInteraction();
        }
    }
}
