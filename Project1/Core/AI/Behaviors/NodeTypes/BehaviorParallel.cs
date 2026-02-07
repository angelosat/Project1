using Project1.Core.AI;
using Project1.Core.Entities.Actors;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors.NodeTypes
{
    class BehaviorParallel : BehaviorComposite
    {
        public BehaviorParallel(params Behavior[] children)
        {
            this.Children = new List<Behavior>(children);
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            var success = true;
            var running = false;
            foreach(var child in this.Children)
            {
                var result = child.Tick(parent, state);
                success &= result == BehaviorState.Success;
                running |= result == BehaviorState.Running;
            }
            if (running)
                return BehaviorState.Running;
            return success ? BehaviorState.Success : BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorParallel();
        }
    }
}
