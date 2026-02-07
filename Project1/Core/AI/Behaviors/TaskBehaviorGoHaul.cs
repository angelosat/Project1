using System.Collections.Generic;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core
{
    class TaskBehaviorGoHaul : BehaviorExecutePlan
    {
        public override string Name { get; } = "Picking up item";

        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnTargetDespawned();
            yield return new BehaviorResolvePath(PathEndMode.Any)
                .FailOnInvalidInteraction(this.Actor, this.Plan);
            yield return new BehaviorResolveInteraction();
        }
        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
        }
    }
}
