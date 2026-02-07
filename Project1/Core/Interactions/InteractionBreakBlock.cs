using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Core.Materials;
using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Interactions;
using Project1.Core.Materials;
using Project1.Core;
using Project1.Core.Simulation;

namespace Project1.Core.Interactions
{
    public class InteractionBreakBlockLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            Cell _cellCached;
            internal Cell Cell => _cellCached ??= this.Actor.Map.GetCell(this.Target.Global);
            public override float ProgressPercentage => 1 - (float)this.Cell.HitPoints / Cell.HitPointsMax;
        }
        public override void ApplyWork(InteractionContext ctx, int workAmount) => ApplyWork((Context)ctx, workAmount);
        public override bool CanFinish(InteractionContext ctx) => CanFinish((Context)ctx);
        public override bool CanPerform(InteractionContext ctx) => CanPerform((Context)ctx);
        protected override InteractionContext CreateContextInternal() => new Context();
        static bool CanPerform(Context ctx)
        {
            return ctx.Cell.Block is not BlockAir;
        }
        static bool CanFinish(Context ctx)
        {
            var global = ctx.Target.Global;
            var actor = ctx.Actor;
            var objects = actor.Map.GetObjects(global.Above());
            return !objects.Any();
        }
        static void ApplyWork(Context ctx, int workAmount) => ctx.Actor.Map.ApplyBlockWork(ctx.Target.Global, -workAmount);
        
    }
    class InteractionBreakBlock : InteractionToolUse
    {
        Cell _cellCached;
        Cell Cell => _cellCached ??= this.Actor.Map.GetCell(this.Target.Global);
        Block Block => this.Cell.Block;
        MaterialDef Material => this.Cell.Material;

        protected override float WorkDifficulty => this.Material.Density;
        
        public InteractionBreakBlock() : base("MineDig")
        {
        }
        protected override void Init()
        {
            var matType = this.Material.Type;
            if (matType == MaterialTypeDefOf.Soil)
                this.Name = "Digging";
            else if (matType == MaterialTypeDefOf.Stone || matType == MaterialTypeDefOf.Metal)
                this.Name = "Mining";

            this.EmitterStrike.Texture = Block.Atlas.Texture;
            this.ParticleRects = this.GetParticleRects();
        }

        protected override Color GetParticleColor()
        {
            return Color.White;
        }

        protected override List<Rectangle> GetParticleRects()
        {
            return this.Block.GetParticleRects(25);
        }

        //protected override SkillDef GetSkill()
        //{
        //    var matType = this.Material.Type;
        //    if (matType == MaterialTypeDefOf.Soil)
        //        return SkillDefOf.Digging;
        //    else if (matType == MaterialTypeDefOf.Stone || matType == MaterialTypeDefOf.Metal)
        //        return SkillDefOf.Mining;
        //    throw new Exception();
        //}

        //protected override ToolUseDef GetToolUse()
        //{
        //    var matType = this.Material.Type;
        //    if (matType == MaterialTypeDefOf.Soil)
        //        return ToolUseDefOf.Digging;
        //    else if (matType == MaterialTypeDefOf.Stone || matType == MaterialTypeDefOf.Metal)
        //        return ToolUseDefOf.Mining;
        //    throw new Exception();
        //}
    }
}
