namespace Start_a_Town_.Components.Interactions
{
    class UnequipItem : Interaction
    {
        public UnequipItem() : base("Unequipping", 0) { }

        public override void Perform()
        {
            var a = this.Actor;
            var t = this.Target;
            a.Inventory.Unequip(t.Object);
        }

        public override object Clone()
        {
            return new UnequipItem();
        }
    }
}
