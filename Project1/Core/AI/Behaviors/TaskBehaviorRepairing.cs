using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Project1.Framework.Pathing;
using Project1.Core.AI.Behaviors.Pathing;

namespace Start_a_Town_
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
