using Start_a_Town_.Framework.AI.NodeTypes;
using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorMoveTo : BehaviorQueue
    {
        public BehaviorMoveTo(string targetKey, int range)
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorDomain(new IsAt(targetKey),
                    new BehaviorStopMoving()),
                new BehaviorResolvePath(targetKey)//, range)
            };
        }

        public BehaviorMoveTo(TargetArgs targetArgs, int range)
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorDomain(new IsAt(targetArgs),
                    new BehaviorStopMoving()),
                new BehaviorResolvePath(targetArgs)
            };
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            var result = base.Tick(parent, state);
            return result;
        }
    }
}
