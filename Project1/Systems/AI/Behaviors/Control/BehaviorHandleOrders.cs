namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorHandleOrders : Behavior
    {
        BehaviorGetAtNewNew CurrentBehav;
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

            if (state.MoveOrder?.Type == TargetType.Position)
            {
                var destination = state.MoveOrder.Global.Above();
                if (parent.IsAt(destination))
                    return BehaviorState.Running;
                if (parent.CanReach(destination))
                {
                    parent.StopPathing();
                    var target = new TargetArgs(parent.Map, destination);
                    parent.CurrentTask = new AITask() { TargetA = target };
                    this.CurrentBehav = new BehaviorGetAtNewNew(TargetIndex.A, PathEndMode.Exact);
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
