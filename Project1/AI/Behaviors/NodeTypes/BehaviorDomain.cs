namespace Start_a_Town_.AI.Behaviors
{
    public class BehaviorDomain : BehaviorDecorator
    {
        BehaviorCondition Condition;
        public BehaviorDomain(BehaviorCondition condition, Behavior child)
        {
            this.Child = child;
            this.Condition = condition;
        }
        public BehaviorDomain(Behavior child, BehaviorCondition condition)
        {
            this.Child = child;
            this.Condition = condition;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var cond = this.Condition.Execute(parent, state);
            if (cond == BehaviorState.Fail)
                return BehaviorState.Fail;
            else
                return this.Child.Execute(parent, state);
        }
       
        public override object Clone()
        {
            return new BehaviorDomain(this.Condition, this.Child.Clone() as Behavior);
        }
    }
}
