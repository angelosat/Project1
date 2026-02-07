using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Core.AI.Behaviors.Conditionals
{
    class ConditionAll : BehaviorCondition
    {
        List<BehaviorCondition> List = new List<BehaviorCondition>();
        public ConditionAll(params BehaviorCondition[] conditions)
        {
            this.List = new List<BehaviorCondition>(conditions);
        }
        public override bool Evaluate(GameObject agent, AIState state)
        {
            //foreach (var c in this.List)
            //    if (!c.Evaluate(agent, state))
            //        return false;
            foreach (var c in this.List)
                if (c.Tick(agent as Actor, state) != BehaviorState.Success)
                    return false;
            return true;
        }
    }
}
