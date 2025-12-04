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
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            this.Child.Execute(parent, state);
            var eval = this.Condition.Evaluate(parent, state);
            if (eval)
                return BehaviorState.Success;
            else
                return BehaviorState.Running;
        }
        public override void Write(System.IO.BinaryWriter w)
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
