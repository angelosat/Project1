namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorRepeat : Behavior
    {
        Behavior Child;
        BehaviorCondition Condition;
        
        public BehaviorRepeat(Behavior child, BehaviorCondition condition)
        {
            this.Child = child;
            this.Condition = condition;
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            this.Child.Tick(parent, state);
            var eval = this.Condition.Evaluate(parent, state);
            if (eval)
                return BehaviorState.Success;
            else
                return BehaviorState.Running;
        }
        public override void Write(IDataWriter w)
        {
            this.Child.Write(w);
        }
        public override void Read(IDataReader r)
        {
            this.Child.Read(r);
        }
        public override object Clone()
        {
            return new BehaviorRepeat(this.Child.Clone() as Behavior, this.Condition);
        }
    }
}
