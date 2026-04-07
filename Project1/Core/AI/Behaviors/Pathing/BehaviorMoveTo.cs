using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Behaviors.Conditionals;
using Project1.Core.Entities.Actors;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors.Pathing
{
    class BehaviorMoveTo : BehaviorQueue
    {
        public BehaviorMoveTo(string targetKey, int range)
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorDomain(new IsAt(targetKey),
                    new BehaviorStopMoving()),
                new BehaviorResolvePath(targetKey)
            };
        }

        public BehaviorMoveTo(InteractionTarget targetArgs, int range)
        {
            this.Children =
            [
                new BehaviorDomain(new IsAt(targetArgs),
                    new BehaviorStopMoving()),
                new BehaviorResolvePath(targetArgs)
            ];
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            var result = base.Tick(parent, state);
            return result;
        }
    }
}
