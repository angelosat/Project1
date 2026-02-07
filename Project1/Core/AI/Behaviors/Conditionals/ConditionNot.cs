using Project1.Core.AI;
using Project1.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Core.AI.Behaviors.Conditionals
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
