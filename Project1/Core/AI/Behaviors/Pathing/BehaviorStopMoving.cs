using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Entities.Actors;

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
