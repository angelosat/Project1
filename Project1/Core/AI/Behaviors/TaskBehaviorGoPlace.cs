using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Behaviors.Pathing;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors
{
    class TaskBehaviorGoPlace : BehaviorExecutePlan
    {
        public override string Name { get; } = "Delivering item";

        protected override IEnumerable<Behavior> GetSteps()
        {
            var index = TargetIndex.A;
            yield return new BehaviorResolvePath(index, PathEndMode.Any).FailOnInvalidInteraction(this.Actor, this.Plan);
            yield return new BehaviorResolveInteraction();// InteractionDefOf.Place, countInd: index);
        }
        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
        }
    }
}
