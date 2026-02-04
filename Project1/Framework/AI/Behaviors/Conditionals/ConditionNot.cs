using Project1.Framework.Entities;
using Start_a_Town_.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Framework.AI.Behaviors.Conditionals
{
    public class ConditionNot : BehaviorCondition
    {
        BehaviorCondition Condition;
        public ConditionNot(BehaviorCondition condition)
        {
            this.Condition = condition;
        }
        public override bool Evaluate(GameObject agent, AIState state)
        {
            return !this.Condition.Evaluate(agent, state);
        }
    }
}
