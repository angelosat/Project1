using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorGoConstruct : BehaviorExecutePlan
    {
        public override string Name { get; } = "Finishing Construction";

        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnNoConstructionDesignation();
            yield return new BehaviorResolvePath(PathEndMode.Touching)
                .FailOnPreInteractionCheck(this.Actor, this.Plan);
            yield return new BehaviorResolveInteraction();
        }
        protected override bool ReserveExtra()
        {
            return this.ReserveAll();
            return this.Reserve(TargetIndex.A);
        }
    }
}
