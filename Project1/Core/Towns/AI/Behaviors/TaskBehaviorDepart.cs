using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Behaviors.Pathing;
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
