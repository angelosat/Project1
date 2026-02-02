using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
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
                //!this.Task.Tool.IsForbidden &&
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
    //class PlanBehaviorSimpleInteraction : BehaviorExecutePlan
    //{
    //    protected override IEnumerable<Behavior> GetSteps()
    //    {
    //        var target = this.Plan.GetTarget(TargetIndex.A);
    //        var ctx = this.Plan.Def.Interaction.CreateContext(this.Actor, target);
    //        yield return new BehaviorResolvePath(TargetIndex.A)
    //            .FailOn(() => !this.Plan.Def.Interaction.Logic.CanPerform(ctx));
    //        yield return new BehaviorResolveInteraction(TargetIndex.A);
    //    }
    //}
}
