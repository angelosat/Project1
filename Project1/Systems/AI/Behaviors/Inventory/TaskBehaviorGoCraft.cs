using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorGoCraft : BehaviorExecutePlan
    {
        public override string Name { get; } = "Crafting";

        protected override IEnumerable<Behavior> GetSteps()
        {
            //yield return new BehaviorResolvePath(PathEndMode.Any);
            yield return new BehaviorResolvePath(PathEndMode.InteractionSpot);
            yield return new BehaviorResolveInteraction();
            //var index = TargetIndex.A;
            //yield return new BehaviorResolvePath(index, PathEndMode.Any);
            //yield return new BehaviorResolveInteraction(index, () => new InteractionCraftingNew());
        }
        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
            return this.Reserve(TargetIndex.A);
        }
    }
}
