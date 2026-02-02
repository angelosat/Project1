using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Start_a_Town_.AI.Behaviors;
using Project1.Core.Interactions;

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
            yield return new BehaviorResolveInteraction(InteractionDefOf.Place, countInd: index);// index, () => new InteractionPlaceItem(this.Actor.CurrentTask.GetAmount(index)));
        }
        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
            //return this.Reserve(TargetIndex.A);
        }
    }
}
