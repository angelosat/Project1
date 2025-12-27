using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorGoPlace : BehaviorExecutePlan
    {
        public override string Name { get; } = "Delivering item";

        protected override IEnumerable<Behavior> GetSteps()
        {
            var index = TargetIndex.A;
            yield return new BehaviorResolvePath(index, PathEndMode.Any);
            //yield return new BehaviorInteractionNew(index, () => new InteractionPlaceItem(this.Actor.CurrentTask.GetAmount(index)));
            yield return new BehaviorBeginInteraction(InteractionDefOf.Place, countInd: index);// index, () => new InteractionPlaceItem(this.Actor.CurrentTask.GetAmount(index)));
        }
        protected override bool InitExtraReservations()
        {
            return this.Reserve(TargetIndex.A);
        }
    }
}
