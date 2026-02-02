using Start_a_Town_.Framework.AI.NodeTypes;
using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class TaskBehaviorDepart : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            //yield return BehaviorHelper.MoveTo(TargetIndex.A);
            //yield return new BehaviorResolveInteraction(TargetIndex.A, () => new InteractionDepart());

            yield return new BehaviorResolvePath(PathEndMode.Exact);
            yield return new BehaviorResolveInteraction();
        }
    }
}
