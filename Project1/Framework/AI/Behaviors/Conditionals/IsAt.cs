using Project1.Framework.Base;
using Project1.Framework.Entities;
using Start_a_Town_.AI;

namespace Project1.Framework.AI.Behaviors.Conditionals
{
    class IsAt : BehaviorCondition
    {
        TargetArgs Target;
        string VariableName;
        public IsAt(string variableName)
        {
            this.VariableName = variableName;
        }
        public IsAt(TargetArgs target)
        {
            this.Target = target;
        }
        
        public override bool Evaluate(GameObject agent, AIState state)
        {
            var target = this.Target ?? state.Blackboard[this.VariableName] as TargetArgs;
            var res = agent.IsInInteractionRange(target);
            return res;
        }
    }
}
