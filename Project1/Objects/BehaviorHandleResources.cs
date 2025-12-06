using Start_a_Town_.AI;

namespace Start_a_Town_
{
    internal class BehaviorHandleResources : Behavior
    {
        public override object Clone()
        {
            return new BehaviorHandleResources();
        }

        public override BehaviorState Tick(Actor parent, AIState state)
        {
            return BehaviorState.Fail;
        }
    }
}