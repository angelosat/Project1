using Project1.Core.AI;
using Project1.Core.Entities.Actors;
using Project1.Framework.IO;

namespace Project1.Core.AI.Behaviors.NodeTypes
{
    class BehaviorSucceeder : Behavior
    {
        Behavior Child;
        public BehaviorSucceeder(Behavior child)
        {
            this.Child = child;
        }
        public BehaviorSucceeder()
        {

        }
        public override BehaviorState Tick(Actor parent, AIState state)
        {
            if (this.Child == null)
                return BehaviorState.Success;
            var result = this.Child.Tick(parent, state);
            if (result == BehaviorState.Running)
                return BehaviorState.Running;
            return BehaviorState.Success;
        }
        public override void Write(IDataWriter w)
        {
            if (this.Child != null)
                this.Child.Write(w);
        }
        public override void Read(IDataReader r)
        {
            if (this.Child != null)
                this.Child.Read(r);
        }
        public override object Clone()
        {
            return new BehaviorSucceeder(this.Child);
        }
    }
}
