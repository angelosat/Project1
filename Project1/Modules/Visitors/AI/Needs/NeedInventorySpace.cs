namespace Start_a_Town_
{
    class NeedInventorySpace : Need
    {
        public NeedInventorySpace(Actor parent) : base(parent)
        {
        }
        // TODO Move this to the def
        public override void Tick(GameObject parent)
        {
            var inv = parent.Inventory;
            var p = inv.PercentageFull;
            this.Value = 1 - p * p;
            this.Value *= 100;
        }
    }
}
