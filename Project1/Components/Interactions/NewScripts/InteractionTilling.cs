namespace Start_a_Town_.Components.Interactions
{
    class InteractionTilling : InteractionPerpetual
    {
        public InteractionTilling() : base("Till") { }

        public override void OnUpdate()
        {
            var a = this.Actor;
            var t = this.Target;
            a.Map.SetBlock(t.Global, BlockDefOf.Farmland, a.Map.GetCell(t.Global).Material, 0);
            this.Finish();
        }

        public override object Clone()
        {
            return new InteractionTilling();
        }
    }
}
