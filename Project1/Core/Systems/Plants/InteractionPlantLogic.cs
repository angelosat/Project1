using Project1.Core.Blocks;
using Project1.Core.Interactions;
using Project1.Core.Simulation;

namespace Project1.Core.Systems.Plants
{
    internal class InteractionPlantLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            Cell _cell;
            internal Cell Cell => this._cell ??= this.Target.Cell;
        }
        protected override InteractionContext CreateContextInternal() => new Context();
        public override bool CanPerform(InteractionContext ctx)
        {
            var typedctx = (Context)ctx;
            if (typedctx.Cell.Block.BlockDef != BlockDefOf.Farmland)
                return false;
            return true;
            //return ctx.Actor.Map.Town.GrowingManager.IsValidPlantingTarget(ctx.Target.Global);
        }
        internal override void OnFinish(Interaction i)
        {
            InteractionHelpers.TryDepositCarriedItemInsideBlockOrSpawn(i);
        }
    }
}
