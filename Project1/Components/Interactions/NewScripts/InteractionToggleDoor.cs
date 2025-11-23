namespace Start_a_Town_
{
    public class InteractionToggleDoor : Interaction
    {
        public InteractionToggleDoor() : base("Open/close") { }
       
        internal override void InitAction()
        {
            var actor = this.Actor;
            var target = this.Target;
            base.InitAction();
            //var doorBase = Cell.GetOrigin(actor.Map, target.Global);
            //BlockDoor.Toggle(actor.Map, doorBase);
            BlockDoor.Toggle(actor.Map, target.Global);
        }
        public override object Clone()
        {
            return new InteractionToggleDoor();
        }
    }
}
