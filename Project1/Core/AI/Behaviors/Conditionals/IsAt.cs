using Project1.Core.AI;
using Project1.Core.Entities;

namespace Project1.Core.AI.Behaviors.Conditionals
{
    class IsAt : BehaviorCondition
    {
        InteractionTarget Target;
        string VariableName;
        public IsAt(string variableName)
        {
            this.VariableName = variableName;
        }
        public IsAt(InteractionTarget target)
        {
            this.Target = target;
        }
        
        public override bool Evaluate(GameObject agent, AIState state)
        {
            var target = this.Target ?? state.Blackboard[this.VariableName] as InteractionTarget;
            var res = agent.IsInInteractionRange(target);
            return res;
        }
    }
}
