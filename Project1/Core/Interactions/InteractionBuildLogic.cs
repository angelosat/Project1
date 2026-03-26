using Project1.Core.Blocks;
using Project1.Core.Blocks.Construction;
using Project1.Core.Simulation;
using System.Linq;

namespace Project1.Core.Interactions
{
    class InteractionBuildLogic : InteractionLogic
    {
        public sealed class Context : InteractionContext
        {
            BlockConstructionComp _cachedComp;
            public BlockConstructionComp CachedComp => this._cachedComp ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockConstructionComp>();
            public override float ProgressBarPercentage => this.CachedComp.Progress.Percentage;
        }
        protected override Context CreateContextInternal() => new();
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
            //map.RemoveBlockEntity(comp.Parent); // the entity is removed by mapedit
            var args = comp.Args;

            MapEdit.Paint(MapEditContext.Simulation, map, [origin], args.BlockDef.Block, args.Material, 0, 0, args.Orientation);

            if(map.Query(origin).BlockEntity is BlockEntity newBuilding)
                newBuilding.GetComp<BlockBuildingComp>().SetIngredient(args.Refinement, args.Amount);

            //var edit = new MapEdit(map, MapEditContext.Simulation);
            //edit.ReplaceWithoutEntity(origin, args.BlockDef.Block, args.Material, 0, 0, args.Orientation);
            //if (block.TryCreateNewBlockEntity(map, origin, args.Orientation) is BlockEntity be)
            //{
            //    be.GetComp<BlockBuildingComp>().IngredientUsed = comp.Requirement.refinement;
            //    edit.AddEntity(be);
            //}
            //edit.Flush();
        }
    }
}
