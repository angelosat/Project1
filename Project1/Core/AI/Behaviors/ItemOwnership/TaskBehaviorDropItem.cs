using System.Collections.Generic;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.AI.Behaviors.ItemOwnership
{
    class TaskBehaviorDropItem : BehaviorExecutePlan
    {
        public override string Name => "Dropping Item";
           
        protected override IEnumerable<Behavior> GetSteps()
        {
            //yield return new BehaviorResolveInteraction(this.Plan.TargetA, new DropInventoryItem());
            yield return new BehaviorResolveInteraction();
        }
        public override bool HasFailedOrEnded()
        {
            return this.Actor.GetPossesions().Contains(this.Plan.TargetA.Object);
        }
    }
}
