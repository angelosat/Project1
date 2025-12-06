namespace Start_a_Town_
{
    class NeedInventorySpace : NeedWorker 
    {
        protected override void TickExtra(Need need)
        {
            var actor = need.Parent;
            var inv = actor.Inventory;
            var p = inv.PercentageFull;
            need.Value = 1 - p * p;
            need.Value *= 100;
        }
    }
    //class NeedInventorySpace : Need
    //{
    //    public NeedInventorySpace(Actor parent) : base(parent)
    //    {
    //    }
    //    // TODO Move this to the def
    //    public override void Tick(GameObject actor)
    //    {
    //        //var actor = need.Parent;
    //        var inv = actor.Inventory;
    //        var p = inv.PercentageFull;
    //        this.Value = 1 - p * p;
    //        this.Value *= 100;
    //    }
    //}
}
