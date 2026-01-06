using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Interactions
{
    class InteractionConstructLogic : InteractionLogic
    {
        public sealed class Context : InteractionContext
        {
            BlockConstructionComp _cachedComp;
            public BlockConstructionComp CachedComp => this._cachedComp ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockConstructionComp>();
            public override float ProgressPercentage => this.CachedComp.Progress.Percentage;
        }
        protected override InteractionContext CreateContextInternal() => new Context();
        public override bool CanPerform(InteractionContext ctx) => this.CanPerform((Context)ctx);
        public override bool CanFinish(InteractionContext ctx) => this.CanFinish((Context)ctx);
        public override void ApplyWork(InteractionContext ctx, int workAmount) => this.ApplyWork((Context)ctx, workAmount);
        bool CanPerform(Context ctx)
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

        bool CanFinish(Context ctx)
        {
            var target = ctx.Target;
            var comp = ctx.CachedComp;
            if (comp.Parent.CellsOccupied.Any(c => comp.Parent.Map.GetEntitiesAt(c).Any()))
                return false;
            return true;
        }
        void ApplyWork(Context ctx, int workAmount)
        {
            ctx.CachedComp.Advance(workAmount);
            //PacketsConstruction.Sync(ctx.CachedComp);
        }
        
    }
    class InteractionConstruct : InteractionToolUse
    {
        public InteractionConstruct()
            : base("Construct")
        {
        }
        protected override float WorkDifficulty { get; } = 1;
        protected override void OnApplyWork(int workAmount)
        {
            this.Def.Logic.ApplyWork(this.Context, workAmount);
        }

        protected override Color GetParticleColor() => default;

        protected override List<Rectangle> GetParticleRects() => null;

        //protected override SkillDef GetSkill() => SkillDefOf.Construction;

        //protected override ToolUseDef GetToolUse() => ToolUseDefOf.Building;
    }
}
