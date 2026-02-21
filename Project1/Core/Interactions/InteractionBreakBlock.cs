using Project1.Core.Blocks;
using Project1.Core.Simulation;

namespace Project1.Core.Interactions
{
    public class InteractionBreakBlockLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            IBlockToken _cachedToken;
            Cell _cellCached;
            BlockDef _initialBlock;
            internal Cell Cell => _cellCached ??= this.Actor.Map.GetCell(this.Target.Global);
            internal IBlockToken CachedToken => _cachedToken ??= this.Actor.Map.GetBlockToken(this.Target.Global);
            internal BlockDef InitialBlock => _initialBlock ??= this.Cell.Block.BlockDef;
            //public override float ProgressPercentage => 1 - (float)this.Cell.HitPoints / Cell.HitPointsMax;
            public override float ProgressPercentage => (1 - this.CachedToken?.HealthPercentage) ?? 0;
        }
        public override void ApplyWork(InteractionContext ctx, int workAmount) => ApplyWork((Context)ctx, workAmount);
        protected override InteractionContext CreateContextInternal() => new Context();
        static void ApplyWork(Context ctx, int workAmount) => ctx.Actor.Map.ApplyBlockWork(ctx.Target.Global, -workAmount);
    }
}
