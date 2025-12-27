using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class BehaviorDropCarried : BehaviorExecutePlan
    {
        public override string Name => "Dropping carried";
       
        protected override IEnumerable<Behavior> GetSteps()
        {
            var target = this.Plan.TargetA;
            var amount = this.Plan.AmountA;
            yield return new BehaviorResolvePath(target);
            yield return new BehaviorBeginInteraction(target, new UseHauledOnTarget(amount));
        }
    }
}
