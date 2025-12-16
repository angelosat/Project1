using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class BehaviorDropCarried : BehaviorPerformTask
    {
        public override string Name => "Dropping carried";
       
        protected override IEnumerable<Behavior> GetSteps()
        {
            var target = this.Task.TargetA;
            var amount = this.Task.AmountA;
            yield return new BehaviorGetAtNewNew(target);
            yield return new BehaviorInteractionNew(target, new UseHauledOnTarget(amount));
        }
    }
}
