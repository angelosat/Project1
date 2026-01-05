namespace Start_a_Town_
{
    class InteractionEquip : InteractionPerpetual
    {
        static public int ID = "Equip".GetHashCode();

        public InteractionEquip()
            : base("Equip")
        {
            this.CrossFadeAnimationLength = 25;
        }

        protected override void Done()
        {
            if (this.Actor.Net.IsClient)
                return;
            var a = this.Actor;
            var t = this.Target;
            //GearComponent.EquipToggle(a, t.Object as Entity);
            this.Actor.Gear.EquipToggle(t.Object as Entity);
            this.Finish();
        }
    }
}
