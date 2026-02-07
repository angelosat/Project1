using System.Collections.Generic;
using Project1.Core.Interactions;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core
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
