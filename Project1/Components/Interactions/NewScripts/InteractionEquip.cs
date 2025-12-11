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

        //protected override void Start()
        //{
        //    var a = this.Actor;
        //    var t = this.Target;
        //    return;
        //    a.CrossFade(this.Animation, false, 25);
        //}

        public override void OnUpdate()
        {
            var a = this.Actor;
            var t = this.Target;
            GearComponent.EquipToggle(a, t.Object as Entity);
            this.Finish();
        }

        public override object Clone()
        {
            return new InteractionEquip();
        }
    }
}
