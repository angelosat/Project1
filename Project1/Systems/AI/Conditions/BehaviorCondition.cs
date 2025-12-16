using System;

namespace Start_a_Town_.AI.Behaviors
{
    public class BehaviorCondition : Behavior
    {
        readonly Func<GameObject, AIState, bool> Condition;
        public BehaviorCondition()
        {

        }
        public BehaviorCondition(Func<GameObject, AIState, bool> condition)
        {
            this.Condition = condition;
        }
        public virtual bool Evaluate(GameObject agent, AIState state)
        {
            return this.Condition(agent, state);
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            var result = this.Evaluate(parent, state);
            return result ? BehaviorState.Success : BehaviorState.Fail;
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
