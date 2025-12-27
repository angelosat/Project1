using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorStoreInInventory : BehaviorExecutePlan
    {
        public override string Name { get; } = "Storing item in inventory";

        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(TargetIndex.A, PathEndMode.Any);
            //yield return new BehaviorInteractionNew(index, () => new InteractionHaul(this.Actor.CurrentTask.GetAmount(index)));
            //yield return new BehaviorInteractionNew(index, () => new InteractionStoreHauled());
            yield return new BehaviorResolveInteraction(InteractionDefOf.Pick, countInd: TargetIndex.A);
            yield return new BehaviorResolveInteraction(InteractionDefOf.Store);
        }
        protected override bool InitExtraReservations()
        {
            return this.Reserve(TargetIndex.A);
        }
    }
}
