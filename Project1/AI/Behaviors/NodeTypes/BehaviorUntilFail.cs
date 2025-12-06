namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorUntilFail : Behavior
    {
        protected Behavior Child;
        public BehaviorUntilFail(Behavior child)
        {
            this.Child = child;
        }
        public BehaviorUntilFail()
        {

        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            var result = this.Child.Tick(parent, state);
            return result == BehaviorState.Fail ? BehaviorState.Success : BehaviorState.Running;
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
            return new BehaviorUntilFail(this.Child);
        }
    }
}
