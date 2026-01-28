using Start_a_Town_.AI.Behaviors;
using System.Collections.Generic;

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
            //return this.Reserve(this.Plan.TargetA, this.Plan.AmountA);
        }
    }
}
