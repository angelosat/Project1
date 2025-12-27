using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorDeconstruct : BehaviorExecutePlan
    {
        public const TargetIndex DeconstructInd = TargetIndex.A;
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnNoDesignation(DeconstructInd, DesignationDefOf.Deconstruct);
            this.FailOnCellStandedOn(DeconstructInd);
            yield return new BehaviorGrabTool();
            yield return new BehaviorResolvePath(DeconstructInd);
            yield return new BehaviorResolveInteraction(DeconstructInd, () => new InteractionDeconstruct()); //()=>new InteractionDeconstruct());
        }
        protected override bool InitExtraReservations()
        {
            return this.Reserve(this.Plan.GetTarget(DeconstructInd), 1);
        }
    }
}
