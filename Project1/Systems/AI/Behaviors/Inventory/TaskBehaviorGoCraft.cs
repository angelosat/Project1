using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Crafting;

namespace Start_a_Town_
{
    class TaskBehaviorGoCraft : BehaviorExecutePlan
    {
        public override string Name { get; } = "Crafting";

        protected override IEnumerable<Behavior> GetSteps()
        {
            var index = TargetIndex.A;
            yield return new BehaviorResolvePath(index, PathEndMode.Any);
            //yield return new BehaviorInteractionNew(index, () => new InteractionCrafting(this.Actor.CurrentTask.GetAmount(index)));
            yield return new BehaviorResolveInteraction(index, () => new InteractionCraftingNew());
        }
        protected override bool InitExtraReservations()
        {
            return this.Reserve(TargetIndex.A);
        }
    }
}
