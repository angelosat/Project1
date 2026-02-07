using System.Collections.Generic;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Interactions;

namespace Project1.Core.AI.Behaviors.ItemOwnership
{
    class BehaviorUnequip : BehaviorExecutePlan
    {
        public override string Name => "Unequipping";
         
        protected override IEnumerable<Behavior> GetSteps()
        {
            //yield return new BehaviorInteractionNew(this.Task.TargetA, new UnequipItem());
            //yield return new BehaviorInteractionNew(TargetIndex.A, new InteractionUnequip());
            yield return new BehaviorResolveInteraction(InteractionDefOf.Unequip);
        }
    }
}
