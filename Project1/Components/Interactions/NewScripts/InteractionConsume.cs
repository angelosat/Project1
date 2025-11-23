namespace Start_a_Town_.Components.Interactions
{
    public class InteractionConsume : Interaction
    {
        readonly ConsumableComponent Comp;
        public InteractionConsume(ConsumableComponent comp)
            : base(
                "Consume",
                0)
        {
            this.Verb = "Consuming";
            this.Comp = comp;
        }
        public override void Perform()
        {
            var actor = this.Actor;
            var target = this.Target;
            var slot = target.Slot;
            if(slot==null)
                return;
            if (slot.Object == null)
                return;
            
            this.Comp.Consume(actor);
            target.Slot.Consume(1);

        }

        public override object Clone()
        {
            return new InteractionConsume(this.Comp);
        }
    }
}
