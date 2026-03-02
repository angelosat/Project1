using Project1.Core.Blocks;
using Project1.Core.Simulation;

namespace Project1.Core.Interactions
{
    public class InteractionBreakBlockLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            IBlockHealth _blockHealth;
            Cell _cellCached;
            BlockDef _initialBlock;
            internal Cell Cell => _cellCached ??= this.Actor.Map.GetCell(this.Target.Global);
            internal IBlockHealth BlockHealth => _blockHealth ??= this.Actor.Map.GetBlockHealth(this.Target.Global);
            internal BlockDef InitialBlock => _initialBlock ??= this.Cell.Block.BlockDef;
            public override float ProgressPercentage => (1 - this.BlockHealth?.HealthPercentage) ?? 0;
        }
        public override void ApplyWork(InteractionContext ctx, int workAmount) => ApplyWork((Context)ctx, workAmount);
        protected override InteractionContext CreateContextInternal() => new Context();
        static void ApplyWork(Context ctx, int workAmount) => ctx.Actor.Map.ApplyBlockDamage(ctx.Target.Global, -workAmount);
    }
}
