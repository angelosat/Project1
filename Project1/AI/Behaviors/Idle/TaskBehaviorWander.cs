using System.Collections.Generic;

namespace Start_a_Town_
{
    class TaskBehaviorWander : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var parent = this.Actor;
            var task = this.Task;
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
