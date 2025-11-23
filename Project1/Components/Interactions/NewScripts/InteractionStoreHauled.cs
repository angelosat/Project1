using Start_a_Town_.Components;

namespace Start_a_Town_
{
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
            var actor = this.Actor;
            var target = this.Target;
            var cachedObject = target.Object;
            actor.StoreCarried();
            actor.Log.Write(string.Format("Stored {0} in inventory", cachedObject));
        }

        public override object Clone()
        {
            return new InteractionStoreHauled();
        }
    }
}
