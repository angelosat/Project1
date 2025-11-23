namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorStopMoving : Behavior
    {
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            parent.MoveToggle(false);
                return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorStopMoving();
        }
    }
}
