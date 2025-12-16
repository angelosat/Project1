namespace Start_a_Town_.AI.Behaviors
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
