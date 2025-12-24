using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorGoHaul : BehaviorExecutePlan
    {
        public override string Name { get; } = "Picking up item";

        protected override IEnumerable<Behavior> GetSteps()
        {
            var index = TargetIndex.A;
            yield return new BehaviorGetAtNewNew(index, PathEndMode.Any);
            //yield return new BehaviorInteractionNew(index, () => new InteractionHaul(this.Actor.CurrentTask.GetAmount(index)));
            yield return new BehaviorInteractionNew(InteractionDefOf.Pick, countInd: index);
        }
        protected override bool InitExtraReservations()
        {
            return this.Reserve(this.Plan.TargetA, this.Plan.AmountA);
        }
    }
}
