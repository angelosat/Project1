namespace Start_a_Town_
{
    class InteractionStoreCarriedLogic : InteractionLogic
    {
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Context.Actor;
            actor.Inventory.StoreHauled();
        }
    }
    class InteractionStoreHauled : Interaction
    {
        public InteractionStoreHauled()
            : base(
            "Put in inventory",
            0
            )
        {
        }
        
        public override void Perform()
        {
            if (Actor.Net.IsClient)
                return;
            var actor = this.Actor;
            var target = this.Target;
            var cachedObject = target.Object;
            //actor.StoreCarried();
            //actor.Log.Write(string.Format("Stored {0} in inventory", cachedObject));
            PacketEntityStoreHauled.Send(actor);
        }
    }
}
