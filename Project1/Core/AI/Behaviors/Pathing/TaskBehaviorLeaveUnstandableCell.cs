using Project1.Core.AI.Behaviors.NodeTypes;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors.Pathing
{
    class TaskBehaviorLeaveUnstandableCell : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOn(() => !this.Actor.Map.IsStandableIn(this.Plan.GetTarget(TargetIndex.A).Global));
            yield return new BehaviorResolvePath(TargetIndex.A, PathEndMode.Exact);
        }
    }
}
