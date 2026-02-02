using Start_a_Town_.Framework.AI.NodeTypes;
using System.Collections.Generic;

namespace Start_a_Town_
{
    class TaskBehaviorWander : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var parent = this.Actor;
            var task = this.Plan;
            yield return new BehaviorCustom(delegate
            {
                parent.Direction = new(task.TargetA.Direction, 0);
                parent.MoveToggle(true);
                parent.WalkToggle(true);
            })
            { SuccessCondition = a => task.TicksCounter >= task.TicksTimeout };
        }
    }
}
