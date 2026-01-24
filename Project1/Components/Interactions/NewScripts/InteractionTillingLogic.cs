namespace Start_a_Town_
{
    class InteractionTillingLogic : InteractionLogic
    {
        public override void ApplyWork(InteractionContext ctx, int workAmount)
        {
            base.ApplyWork(ctx, workAmount);
        }

        public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);

        public override bool CanPerform(InteractionContext ctx)
        {
            var actor = ctx.Actor;
            var target = ctx.Target;
            var manager = actor.Map.Town.GrowingManager;
            var result = manager.IsValidTillingTarget(target.Global);
            return result;
        }

        public override bool WillFinish(InteractionContext ctx, int workAmount)
        {
            return base.WillFinish(ctx, workAmount);
        }

        protected override InteractionContext CreateContextInternal()
        {
            return base.CreateContextInternal();
        }
        internal override void OnFinish(Interaction i)
        {
            var map = i.Actor.Map;
            var global = i.Target.Global;
            //map.SetBlock(global, BlockDefOf.Farmland.Worker, map.GetCell(global).Material, 0);
            MapEdit.Paint(MapEditContext.Simulation, map, [global], BlockDefOf.Farmland.Worker, map.GetCell(global).Material, 0, 0, 0);
        }
        internal override int CalculateMax(InteractionContext ctx) => 50;
        //internal override void Done()
        //{
        //    var a = this.Actor;
        //    if (a.Net.IsClient)
        //        return;
        //    var t = this.Target;
        //    a.Map.SetBlock(t.Global, BlockDefOf.Farmland.Worker, a.Map.GetCell(t.Global).Material, 0);
        //    this.Finish();
        //}
    }
}
