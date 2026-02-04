using Project1.Framework.Entities;
using Project1.Framework.Entities.Actors;
using Start_a_Town_.AI;
using Start_a_Town_.Framework.AI.NodeTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Framework.AI.Behaviors.Conditionals
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
