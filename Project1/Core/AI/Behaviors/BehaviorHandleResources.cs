using Project1.Core.Entities.Actors;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.AI.Behaviors
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