using System;
using Start_a_Town_.Framework.AI.NodeTypes;
using Start_a_Town_.AI;
using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;

namespace Start_a_Town_
{
    class BehaviorItemIsInInventory : Behavior
    {
        private readonly int TargetInd;

        public override object Clone()
        {
            throw new Exception();
        }
        public BehaviorItemIsInInventory(TargetIndex targetInd) : this((int)targetInd)
        {

        }
        public BehaviorItemIsInInventory(TargetArgs item)
        {
            throw new Exception();
        }
        public BehaviorItemIsInInventory(int targetInd)
        {
            this.TargetInd = targetInd;
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            var item = parent.CurrentTask.GetTarget(this.TargetInd);
            return parent.Inventory.Contains(item.Object) ? BehaviorState.Success : BehaviorState.Fail;
        }
    }
}
