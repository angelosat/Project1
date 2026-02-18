using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Entities.Actors;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Framework;

namespace Project1.Core.AI.Behaviors
{
    class BehaviorHandleOrders : Behavior
    {
        BehaviorResolvePath CurrentBehav;
        TargetArgs CurrentMoveOrder = TargetArgs.Null;
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            if (this.CurrentBehav is not null && this.CurrentMoveOrder != TargetArgs.Null && state.MoveOrder == this.CurrentMoveOrder)
            {
                if (this.CurrentBehav.Tick(parent, state) != BehaviorState.Running)
                {
                    this.CurrentBehav = null;
                    state.MoveOrders.Dequeue();
                    this.CurrentMoveOrder = TargetArgs.Null;
                }
                else
                    return BehaviorState.Running;
            }

            if (state.MoveOrder?.Type == TargetType.Cell)
            {
                var destination = state.MoveOrder.Global.Above();
                if (parent.IsAt(destination))
                    return BehaviorState.Running;
                if (parent.CanReach(destination))
                {
                    parent.StopPathing();
                    var target = new TargetArgs(parent.Map, destination);
                    parent.CurrentPlan = new Plan() { TargetA = target };
                    this.CurrentBehav = new BehaviorResolvePath(TargetIndex.A, PathEndMode.Exact);
                    this.CurrentMoveOrder = state.MoveOrder;
                    return BehaviorState.Running;
                }
            }
            return BehaviorState.Fail; // fail only when not awaiting move orders
        }

        public override object Clone()
        {
            return new BehaviorHandleOrders();
        }
    }
}
