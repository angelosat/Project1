using Start_a_Town_.Framework.AI.NodeTypes;
using System.Collections.Generic;
using Project1.Framework.Pathing;
using Project1.Core.AI.Behaviors.Pathing;

namespace Start_a_Town_
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
