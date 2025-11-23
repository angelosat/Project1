using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorDeconstruct : BehaviorPerformTask
    {
        public const TargetIndex DeconstructInd = TargetIndex.A;
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnNoDesignation(DeconstructInd, DesignationDefOf.Deconstruct);
            this.FailOnCellStandedOn(DeconstructInd);
            yield return new BehaviorGrabTool();
            yield return new BehaviorGetAtNewNew(DeconstructInd);
            yield return new BehaviorInteractionNew(DeconstructInd, () => new InteractionDeconstruct()); //()=>new InteractionDeconstruct());
        }
        protected override bool InitExtraReservations()
        {
            return this.Actor.Reserve(this.Task.GetTarget(DeconstructInd), 1);
        }
    }
}
