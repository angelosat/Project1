using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorChopping : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorGrabTool();
            yield return new BehaviorGetAtNewNew(TargetIndex.A);
            //yield return new BehaviorInteractionNew(TargetIndex.A, new InteractionChoppingSimple());
            yield return new BehaviorInteractionNew(TargetIndex.A, new InteractionChop());
        }

        public override bool HasFailedOrEnded()
        {
            var tree = this.Task.TargetA.Object;
            var isvalid =
                //!this.Task.Tool.IsForbidden &&
                !tree.IsForbidden &&
                tree != null && tree.Exists;//&& this.Actor.Map.Town.ChoppingManager.IsChoppingTask(tree);
            /// removed the designation check because the behavior might have been created without a specific designation, such as from a growing zone or to clear area for construction
            return !isvalid;
        }

        protected override bool InitExtraReservations()
        {
            return this.Reserve(TargetIndex.A);
        }
    }
}
