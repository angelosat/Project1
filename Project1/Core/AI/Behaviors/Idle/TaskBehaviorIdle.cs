using Project1.Core.AI.Behaviors.NodeTypes;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors.Idle
{
    class TaskBehaviorIdle : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Plan;
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
