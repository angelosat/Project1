using System.Collections.Generic;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_
{
    class TaskBehaviorDropItem : BehaviorPerformTask
    {
        public override string Name => "Dropping Item";
           
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorInteractionNew(this.Task.TargetA, new DropInventoryItem());
        }
        public override bool HasFailedOrEnded()
        {
            return this.Actor.GetPossesions().Contains(this.Task.TargetA.Object);
        }
    }
}
