using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.AI
{
    class BehaviorPickUpItemNew : BehaviorExecutePlan
    {
        public override string Name => "Picking up item";
       
        protected override IEnumerable<Behavior> GetSteps()
        {
            var item = this.Plan.TargetA;
            yield return new BehaviorResolvePath(item);
            yield return new BehaviorResolveInteraction(item, new InteractionHaul());
            yield return new BehaviorResolveInteraction(item, new InteractionStoreHauled());
        }

        public override bool HasFailedOrEnded()
        {
            return false;
        }
        protected override bool ReserveExtra()
        {
            return this.Reserve(this.Plan.TargetA);
        }
    }
}
