using Project1.Framework.AI.Behaviors.Conditionals;
using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using Start_a_Town_.AI;
using Start_a_Town_.Framework.AI.NodeTypes;
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
                new BehaviorResolvePath(targetKey)//, range)
            };
        }

        public BehaviorMoveTo(TargetArgs targetArgs, int range)
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
