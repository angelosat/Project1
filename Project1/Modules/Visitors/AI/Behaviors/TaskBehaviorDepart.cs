using System.Collections.Generic;

namespace Start_a_Town_
{
    class TaskBehaviorDepart : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return BehaviorHelper.MoveTo(TargetIndex.A);
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionDepart());
        }
    }
}
