namespace Start_a_Town_
{
    class InteractionTilling : InteractionPerpetual
    {
        public InteractionTilling() : base("Till") { }

        public override void OnUpdate()
        {
            var a = this.Actor;
            if (a.Net.IsClient)
                return;
            var t = this.Target;
            a.Map.SetBlock(t.Global, BlockDefOf.Farmland.Worker, a.Map.GetCell(t.Global).Material, 0);
            this.Finish();
        }
    }
}
