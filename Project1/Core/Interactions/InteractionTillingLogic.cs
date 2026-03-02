using Project1.Core.Blocks;
using Project1.Core.Simulation;

namespace Project1.Core.Interactions
{
    class InteractionTillingLogic : InteractionLogic
    {
        public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);

        public override bool CanPerform(InteractionContext ctx)
        {
            var actor = ctx.Actor;
            var target = ctx.Target;
            var manager = actor.Map.Town.GrowingManager;
            var result = manager.IsValidTillingTarget(target.Global);
            return result;
        }

        internal override void OnFinish(Interaction i)
        {
            var map = i.Actor.Map;
            var global = i.Target.Global;
            MapEdit.Paint(MapEditContext.Simulation, map, [global], BlockDefOf.Farmland.Block, map.GetCell(global).Material, 0, 0, 0);
        }
        internal override int CalculateMax(InteractionContext ctx) => 50;
        internal override void OnProgressAdded(Interaction i, int delta)
        {
            var target = i.Context.Target;
            var map = target.Map;
            //i.Context.Target.Map.Events.Post(new BlockHitEvent(target.Block, map, target.Global, delta));
            i.Context.Target.Map.Events.Post(new BlockDamagedEvent(map, target.Global, 0));
        }
    }
}
