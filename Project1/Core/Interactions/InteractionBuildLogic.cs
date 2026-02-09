using System.Linq;
using Project1.Core.Blocks;
using Project1.Core.Simulation;

namespace Project1.Core.Interactions
{
    class InteractionBuildLogic : InteractionLogic
    {
        public sealed class Context : InteractionContext
        {
            BlockConstructionComp _cachedComp;
            public BlockConstructionComp CachedComp => this._cachedComp ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockConstructionComp>();
            public override float ProgressPercentage => this.CachedComp.Progress.Percentage;
        }
        protected override InteractionContext CreateContextInternal() => new Context();
        public override bool CanPerform(InteractionContext ctx) => CanPerform((Context)ctx);
        public override bool CanFinish(InteractionContext ctx) => CanFinish((Context)ctx);
        public override void ApplyWork(InteractionContext ctx, int workAmount) => ApplyWork((Context)ctx, workAmount);
        static bool CanPerform(Context ctx)
        {
            var target = ctx.Target;
            var comp = ctx.CachedComp;
            if (comp.Map is null)
                return false;
            var manager = comp.Parent.Map.Town.ConstructionsManager;
            if (!manager.IsDesignatedConstruction(comp))
                return false;
            if (comp.Parent.CellsOccupied.Any(c => !manager.IsSupported(c)))
                return false;
            return true;
        }

        static bool CanFinish(Context ctx)
        {
            var target = ctx.Target;
            var comp = ctx.CachedComp;
            if (comp.Parent.CellsOccupied.Any(c => comp.Parent.Map.GetEntitiesAt(c).Any()))
                return false;
            return true;
        }
        static void ApplyWork(Context ctx, int workAmount)
        {
            ctx.CachedComp.Advance(workAmount);
            if(ctx.CachedComp.IsFinished && ctx.Actor.Net.IsServer)
                Complete(ctx.CachedComp);
        }

        static void Complete(BlockConstructionComp comp)
        {
            var map = comp.Map;
            map.Events.Post(new ConstructionFinishedEvent(comp));

            var cells = comp.Parent.CellsOccupied;
            var origin = comp.Parent.OriginGlobal;
            // remove block entity first because this implicitly sets all occupied cells to air
            map.RemoveBlockEntity(comp.Parent);
            var args = comp.Args;

            MapEdit.Paint(MapEditContext.Simulation, map, [origin], args.Block.Worker, args.Material, 0, 0, args.Orientation);
            //MapEdit.PaintWithOrigin(MapEditContext.Simulation, map, [origin], args.Block.Worker, args.Material, 0, 0, args.Orientation);
            return;


            foreach (var cell in cells)
                map.SetBlock(cell, args.Block.Worker, args.Material, 0, 0, args.Orientation);
        }
    }
}
