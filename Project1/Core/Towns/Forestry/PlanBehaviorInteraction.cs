using System.Collections.Generic;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.Towns.Forestry
{
    class PlanBehaviorInteraction : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            if(this.Plan.Designation is not null)
                this.FailOnNoDesignation(this.Plan.Designation);
            yield return new BehaviorResolvePath(PathEndMode.Any)
                .FailOnInvalidInteraction(this.Actor, this.Plan);
            yield return new BehaviorResolveInteraction();
        }

        public override bool HasFailedOrEnded()
        {
            var tree = this.Plan.TargetA.Object;
            var isvalid =
                !tree.IsForbidden &&
                tree != null && tree.Exists;//&& this.Actor.Map.Town.ChoppingManager.IsChoppingTask(tree);
            /// removed the designation check because the behavior might have been created without a specific designation, such as from a growing zone or to clear area for construction
            return !isvalid;
        }

        protected override bool ReserveExtra()
        {
            return this.Reserve(TargetIndex.A);
        }
    }
    
}
