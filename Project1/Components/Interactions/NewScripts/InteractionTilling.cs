using Microsoft.Xna.Framework;
using System.Collections.Generic;

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
            //if (!result)
            //    "GIATI".ToConsole();
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
    }
    class InteractionTilling : InteractionToolUse
    {
        public InteractionTilling() : base("Till") 
        {
       

        }

        protected override float WorkDifficulty => 1;

        ProgressInt _progress;
        protected override float Progress => this._progress.Percentage;
        public override float PercentageComplete => this._progress.Percentage;
        protected override void OnInitialize(Actor actor, TargetArgs target)
        {
            // TODO: derive progress max by soil hardness?
            this._progress = new(50); // placeholder
        }
        protected override void Done() 
        {
            var a = this.Actor;
            if (a.Net.IsClient)
                return;
            var t = this.Target;
            a.Map.SetBlock(t.Global, BlockDefOf.Farmland.Worker, a.Map.GetCell(t.Global).Material, 0);
            this.Finish();
        }
        protected override Color GetParticleColor() => default;
        protected override List<Rectangle> GetParticleRects() => null;
        protected override SkillDef GetSkill() => SkillDefOf.Argiculture;
        protected override ToolUseDef GetToolUse() => ToolUseDefOf.Argiculture;

        protected override void OnAddProgress(int workAmount)
        {
            this._progress.Add(workAmount);
        }
    }
}
