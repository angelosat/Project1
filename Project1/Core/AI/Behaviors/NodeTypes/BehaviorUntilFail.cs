using Project1.Core.AI;
using Project1.Core.Entities.Actors;
using Project1.Framework.IO;

namespace Project1.Core.AI.Behaviors.NodeTypes
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
            return new BehaviorUntilFail(this.Child);
        }
    }
}
