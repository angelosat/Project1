using System.Collections.Generic;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class BehaviorUnequip : BehaviorPerformTask
    {
        public override string Name => "Unequipping";
         
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorInteractionNew(this.Task.TargetA, new UnequipItem());
        }
    }
}
