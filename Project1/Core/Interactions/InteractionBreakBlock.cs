using Project1.Core.Blocks;
using Project1.Core.Simulation;

namespace Project1.Core.Interactions
{
    public class InteractionBreakBlockLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            internal MapQuery Query => field ??= new MapQuery(this.Actor.Map, this.Target.Global);
            internal IBlockHealth BlockHealth => field ??= this.Actor.Map.GetBlockHealth(this.Target.Global);
            internal BlockDef InitialBlock => field ??= this.Query.Block.BlockDef;
            public override float ProgressBarPercentage => (1 - this.BlockHealth?.HealthPercentage) ?? 0;
        }
        //class Context : InteractionContext
        //{
        //    internal MapQuery Query => field ??= new MapQuery(this.Actor.Map, this.Target.Global);
        //    internal Cell Cell => field ??= this.Actor.Map.GetCell(this.Target.Global);
        //    internal IBlockHealth BlockHealth => field ??= this.Actor.Map.GetBlockHealth(this.Target.Global);
        //    internal BlockDef InitialBlock => field ??= this.Cell.Block.BlockDef;
        //    public override float ProgressPercentage => (1 - this.BlockHealth?.HealthPercentage) ?? 0;
        //}
        public override void ApplyWork(InteractionContext ctx, int workAmount) => ApplyWork((Context)ctx, workAmount);
        protected override InteractionContext CreateContextInt() => new Context();
        static void ApplyWork(Context ctx, int workAmount) => ctx.Actor.Map.ApplyBlockDamage(ctx.Target.Global, -workAmount);
    }
}
