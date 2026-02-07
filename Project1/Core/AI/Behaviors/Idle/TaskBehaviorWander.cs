using Project1.Core.AI.Behaviors.NodeTypes;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors.Idle
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
