namespace Start_a_Town_
{
    public class InteractionToggleDoor : Interaction
    {
        public InteractionToggleDoor() : base("Open/close") { }

        protected override void OnStart()
        {
            var actor = this.Actor;
            var target = this.Target;
            BlockDoor.Toggle(actor.Map, target.Global);
        }
    }
}
