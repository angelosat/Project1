using System.Collections.Generic;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_
{
    class TaskBehaviorDropItem : BehaviorExecutePlan
    {
        public override string Name => "Dropping Item";
           
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorInteractionNew(this.Plan.TargetA, new DropInventoryItem());
        }
        public override bool HasFailedOrEnded()
        {
            return this.Actor.GetPossesions().Contains(this.Plan.TargetA.Object);
        }
    }
}
