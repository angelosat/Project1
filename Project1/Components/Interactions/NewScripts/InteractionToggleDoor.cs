namespace Start_a_Town_
{
    class InteractionToggleDoorLogic : InteractionLogic
    {
        internal override void OnStart(Interaction i)
        {
            var ctx = i.Context;
            var actor = ctx.Actor;
            var target = ctx.Target;
            BlockDoor.Toggle(actor.Map, target.Global);
        }
    }
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
