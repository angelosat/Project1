namespace Start_a_Town_.AI.Behaviors
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
        public override void Write(System.IO.BinaryWriter w)
        {
            this.Child.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Child.Read(r);
        }
        internal override void ObjectLoaded(GameObject parent)
        {
            this.Child.ObjectLoaded(parent);
        }
    }
}
