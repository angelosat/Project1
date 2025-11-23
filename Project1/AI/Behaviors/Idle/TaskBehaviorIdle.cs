using System.Collections.Generic;

namespace Start_a_Town_
{
    class TaskBehaviorIdle : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Task;
            yield return new BehaviorWait(() => task.TicksWaited >= task.TicksTimeout);
            yield return new BehaviorCustom(delegate
            {
                task.TicksCounter = 0;
                actor.Direction = new(task.TargetA.Direction, 0);
                actor.MoveToggle(true);
                actor.WalkToggle(true);
            })
            {
                Mode = BehaviorCustom.Modes.Continuous,
                SuccessCondition = a => task.TicksCounter >= Ticks.PerSecond 
            };
        }
    }
}
