using Project1.Core.AI.Behaviors.Pathing;
using Project1.Framework.Pathing;
using Start_a_Town_;
using Start_a_Town_.Framework.AI.NodeTypes;
using System.Collections.Generic;

namespace Project1.Core.Towns.AI.Behaviors
{
    class TaskBehaviorDepart : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(PathEndMode.Exact);
            yield return new BehaviorResolveInteraction();
        }
    }
}
