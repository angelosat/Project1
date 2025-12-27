using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorSwitchToggle : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnNoDesignation(TargetIndex.A, DesignationDefOf.Switch);
            yield return new BehaviorResolvePath(TargetIndex.A);
            yield return new BehaviorBeginInteraction(TargetIndex.A, () => new InteractionFlipSwitch());
        }
    }
}
