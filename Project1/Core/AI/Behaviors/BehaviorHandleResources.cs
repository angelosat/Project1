using Start_a_Town_.Framework.AI.NodeTypes;
using Start_a_Town_.AI;
using Project1.Framework.Entities.Actors;

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