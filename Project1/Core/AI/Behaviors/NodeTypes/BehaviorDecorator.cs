using Project1.Core.Entities;
using Project1.Core.Helpers;

namespace Project1.Core.AI.Behaviors.NodeTypes
{
    public abstract class BehaviorDecorator : Behavior
    {
        protected Behavior Child;
        public BehaviorDecorator()
        {

        }
        public BehaviorDecorator(Behavior child)
        {
            this.Child = child;
        }
        public override void Write(IDataWriter w)
        {
            this.Child.Write(w);
        }
        public override void Read(IDataReader r)
        {
            this.Child.Read(r);
        }
        internal override void ObjectLoaded(GameObject parent)
        {
            this.Child.ObjectLoaded(parent);
        }
    }
}
