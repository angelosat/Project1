using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Interactions;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors.ItemOwnership
{
    class BehaviorEquipItemNew : BehaviorExecutePlan
    {
        public override string Name => "Equipping item";
        public BehaviorEquipItemNew()
        {

        }
        //public BehaviorEquipItemNew(AITask task)
        //{
        //    this.Task = task;
        //}
        
        protected override IEnumerable<Behavior> GetSteps()
        { 
            // this behavior assumes the item is equipped from the inventory
            //yield return new BehaviorResolvePath(TargetIndex.A);
            yield return new BehaviorResolveInteraction(InteractionDefOf.Equip);
        }
        protected override bool ReserveExtra()
        {
            return this.Reserve(this.Plan.TargetA, 1);
        }
    }
}
