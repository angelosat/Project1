using System.Collections.Generic;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors;
using Project1.Core.Towns.Designations;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.Towns.Tasks
{
    class TaskBehaviorDeconstruct : BehaviorExecutePlan
    {
        public const TargetIndex DeconstructInd = TargetIndex.A;
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnNoDesignation(DeconstructInd, DesignationDefOf.Deconstruct);
            this.FailOnCellStandedOn(DeconstructInd);
            yield return new BehaviorResolvePath(DeconstructInd);
            yield return new BehaviorResolveInteraction(DeconstructInd, () => new InteractionDeconstruct()); //()=>new InteractionDeconstruct());
        }
        protected override bool ReserveExtra()
        {
            return this.Reserve(this.Plan.GetTarget(DeconstructInd), 1);
        }
    }
}
