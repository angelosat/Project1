namespace Start_a_Town_
{
    class InteractionUnequip : Interaction
    {
        public InteractionUnequip() : base("Unequipping", 0) { }

        public override void Perform()
        {
            var a = this.Actor;
            var t = this.Target;
            a.Inventory.Unequip(t.Object);
        }
    }
}
