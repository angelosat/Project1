using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Behaviors.Pathing;
using System.Collections.Generic;

namespace Project1.Core
{
    class BehaviorBuild : BehaviorExecutePlanNew
    {

    }
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
        }
    }
}
