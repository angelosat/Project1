namespace Start_a_Town_.Components.Interactions
{
    class DropInventoryItem : Interaction
    {
        public DropInventoryItem()
            : base(
            "DropInventoryItem",
            0
            )
        {

        }
       
        public override void Perform()
        {
            this.Actor.Inventory.Drop(this.Target.Object);
        }

        public override object Clone()
        {
            return new DropInventoryItem();
        }
    }
}
