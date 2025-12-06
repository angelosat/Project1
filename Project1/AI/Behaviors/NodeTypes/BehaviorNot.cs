namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorNot : BehaviorCondition
    {
        Behavior Child;
        public BehaviorNot(Behavior child)
        {
            this.Child = child;
        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            var result = this.Child.Tick(parent, state);
            if (result == BehaviorState.Success)
                result = BehaviorState.Fail;
            else if (result == BehaviorState.Fail)
                result = BehaviorState.Success;
            return result;
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
            return new BehaviorNot(this.Child);
        }
    }
}
