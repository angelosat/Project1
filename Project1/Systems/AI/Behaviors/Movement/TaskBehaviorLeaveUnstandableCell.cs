using Project1.Framework.Pathing;
using Start_a_Town_.Framework.AI.NodeTypes;
using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
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
