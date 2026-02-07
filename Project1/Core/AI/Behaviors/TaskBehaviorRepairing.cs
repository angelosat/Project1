using System.Collections.Generic;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core
{
    class TaskBehaviorRepairing : BehaviorExecutePlan
    {
        public override string Name { get; } = "Repairing";

        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(TargetIndex.B, PathEndMode.InteractionSpot);
            yield return new BehaviorResolveInteraction();
        }
        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
        }
    }
}
