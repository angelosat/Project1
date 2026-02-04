using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Project1.Core.Interactions;
using Project1.Framework.Pathing;
using Project1.Core.AI.Behaviors.Pathing;

namespace Start_a_Town_
{
    class TaskBehaviorGoPlace : BehaviorExecutePlan
    {
        public override string Name { get; } = "Delivering item";

        protected override IEnumerable<Behavior> GetSteps()
        {
            var index = TargetIndex.A;
            yield return new BehaviorResolvePath(index, PathEndMode.Any);
            yield return new BehaviorResolveInteraction(InteractionDefOf.Place, countInd: index);// index, () => new InteractionPlaceItem(this.Actor.CurrentTask.GetAmount(index)));
        }
        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
        }
    }
}
