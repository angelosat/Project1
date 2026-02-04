using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Project1.Framework.Pathing;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Framework.Base;

namespace Start_a_Town_
{
    class TaskBehaviorTilling : BehaviorExecutePlan
    {
        public const TargetIndex TargetInd = TargetIndex.A;
        TargetArgs Target { get { return this.Plan.GetTarget(TargetInd); } }
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(PathEndMode.Touching)
                .FailOnInvalidInteraction(this.Actor, this.Plan);
            yield return new BehaviorResolveInteraction();
        }
        protected override bool ReserveExtra()
        {
            return this.Reserve(this.Plan.TargetA, 1);
        }
    }
}
