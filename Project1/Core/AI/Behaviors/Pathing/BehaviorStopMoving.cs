using Project1.Framework.Entities.Actors;
using Start_a_Town_.AI;
using Start_a_Town_.Framework.AI.NodeTypes;

namespace Project1.Core.AI.Behaviors.Pathing
{
    class BehaviorStopMoving : Behavior
    {
        public override BehaviorState Tick(Actor parent, AIState state)
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
