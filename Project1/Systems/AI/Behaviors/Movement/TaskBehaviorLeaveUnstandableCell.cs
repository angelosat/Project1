using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class TaskBehaviorLeaveUnstandableCell : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOn(() => !this.Actor.Map.IsStandableIn(this.Task.GetTarget(TargetIndex.A).Global));
            yield return new BehaviorGetAtNewNew(TargetIndex.A, PathEndMode.Exact);
        }
    }
}
