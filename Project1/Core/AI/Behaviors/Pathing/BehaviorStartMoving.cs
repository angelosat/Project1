using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Entities.Actors;

namespace Project1.Core.AI.Behaviors.Pathing
{
    class BehaviorStartMoving : Behavior
    {
        public bool Sprint;
        public BehaviorStartMoving(bool sprint = true)
        {
            this.Sprint = sprint;
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            if (state.Path == null)
                return BehaviorState.Fail;
            parent.MoveToggle(true);
            parent.WalkToggle(!this.Sprint);
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorStartMoving(this.Sprint);
        }
    }
}
