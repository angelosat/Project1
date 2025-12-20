using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Crafting;

namespace Start_a_Town_
{
    class TaskBehaviorGoPlace : BehaviorPerformTask
    {
        public override string Name { get; } = "Delivering item";

        protected override IEnumerable<Behavior> GetSteps()
        {
            var index = TargetIndex.A;
            yield return new BehaviorGetAtNewNew(index, PathEndMode.Any);
            yield return new BehaviorInteractionNew(index, () => new InteractionPlaceItem(this.Actor.CurrentTask.GetAmount(index)));
        }
        protected override bool InitExtraReservations()
        {
            return this.Reserve(TargetIndex.A);
        }
    }

    class TaskBehaviorGoCraft : BehaviorPerformTask
    {
        public override string Name { get; } = "Crafting";

        protected override IEnumerable<Behavior> GetSteps()
        {
            var index = TargetIndex.A;
            yield return new BehaviorGetAtNewNew(index, PathEndMode.Any);
            //yield return new BehaviorInteractionNew(index, () => new InteractionCrafting(this.Actor.CurrentTask.GetAmount(index)));
            yield return new BehaviorInteractionNew(index, () => new InteractionCraftingNew());
        }
        protected override bool InitExtraReservations()
        {
            return this.Reserve(TargetIndex.A);
        }
    }
}
