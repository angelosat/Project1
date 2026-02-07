using System.Collections.Generic;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core
{
    class TaskBehaviorGoConstruct : BehaviorExecutePlan
    {
        public override string Name { get; } = "Finishing Construction";

        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnNoConstructionDesignation();
            yield return new BehaviorResolvePath(PathEndMode.Touching)
                .FailOnInvalidInteraction(this.Actor, this.Plan);
            yield return new BehaviorResolveInteraction();
        }
        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
            return this.Reserve(TargetIndex.A);
        }
    }
}
