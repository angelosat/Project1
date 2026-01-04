using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.Particles;

namespace Start_a_Town_
{
    public class InteractionBreakBlockLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            Cell _cellCached;
            internal Cell Cell => _cellCached ??= this.Actor.Map.GetCell(this.Target.Global);
            //int? _totalHp;
            //internal int TotalHp => _totalHp ??= this.Cell.Material.BreakResistance * this.Cell.HitPoints;
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
        static void ApplyWork(Context ctx, int workAmount)
        {
            ctx.Actor.Map.ApplyBlockWork(ctx.Target.Global, -workAmount);
            ctx.Actor.Map.Events.Post(new BlockHitEvent(ctx.Target.Map, ctx.Target.Global));
        }
    }
    class InteractionBreakBlock : InteractionToolUse
    {
        //ParticleEmitterSphere EmitterBreak;
        Cell _cellCached;
        Cell Cell => _cellCached ??= this.Actor.Map.GetCell(this.Target.Global);
        Block Block => this.Cell.Block;
        MaterialDef Material => this.Cell.Material;
        float AccumulatedWorkThisBreakStage, AccumulatedWorkTotal;

        protected override float WorkDifficulty => this.Material.Density;
        //float TotalHp;
        //protected override float Progress => this.WorkAppliedTotal / this.TotalHp;
        //protected override SkillAwardTypes SkillAwardType { get; } = SkillAwardTypes.OnFinish;

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

            //var global = this.Target.Global;
            //this.TotalHp = this.Cell.Material.BreakResistance * this.Cell.HitPoints;

            //var blockEmitter = this.Block.GetEmitter();
            //this.EmitterStrike = blockEmitter;
            this.EmitterStrike.Texture = Block.Atlas.Texture;
            this.ParticleRects = this.GetParticleRects();
        }
        protected override void OnAddProgress(int v)
        {
            this.Def.Logic.ApplyWork(this.Context, v);
        }

        //protected override void OnApplyWork(int workAmount)
        //{
        //    this.Def.Logic.ApplyWork(this.Context, workAmount);
        //    //if (this.Target.Cell.HitPoints == 0)
        //    //    this.Done();
        //    return;
        //    this.AccumulatedWorkThisBreakStage += workAmount;
        //    this.AccumulatedWorkTotal += workAmount;
        //    var resistance = this.Cell.Material.BreakResistance;
        //    if (this.AccumulatedWorkThisBreakStage >= resistance)
        //    {
        //        this.AccumulatedWorkThisBreakStage -= resistance;
        //        this.Cell.Damage++;
        //        var vec = this.Target.Global;
        //        this.Actor.Map.GetChunk(vec).InvalidateSlice((byte)vec.Z);
        //        this.AccumulatedWorkThisBreakStage -= resistance;
        //    }
        //}

        protected override void Done()
        {
            var a = this.Actor;
            var t = this.Target;
            //var cell = this.Cell;

            //if (a.Net is Server server && cell.Block.BreakProduct is ItemDef productDef)
            //    server.PopLoot(ItemFactory.CreateFrom(productDef, cell.Material), t.Global, Vector3.Zero);

            a.Map.RemoveBlock(t.Global);
            // test: letting client perform the last interaction tick so that it has a chance to emit particles
            //PacketBreakBlocks.Send(a.Map, [t.Global]);


            //void emitBreak()
            //{
            //    this.EmitterBreak.Emit(Block.Atlas.Texture, this.ParticleRects, Vector3.Zero);
            //    a.Map.ParticleManager.AddEmitter(this.EmitterBreak);
            //}
        }

        protected override Color GetParticleColor()
        {
            return Color.White;
            //return this.Material.Color;
        }

        protected override List<Rectangle> GetParticleRects()
        {
            //return BlockDefOf.Grass.Variations[0].Rectangle.Divide(25);
            //return ItemContent.LogsGrayscale.AtlasToken.Rectangle.Divide(25);
            return this.Block.GetParticleRects(25);
        }

        protected override SkillDef GetSkill()
        {
            var matType = this.Material.Type;
            if (matType == MaterialTypeDefOf.Soil)
                return SkillDefOf.Digging;
            else if (matType == MaterialTypeDefOf.Stone || matType == MaterialTypeDefOf.Metal)
                return SkillDefOf.Mining;
            throw new Exception();
        }

        protected override ToolUseDef GetToolUse()
        {
            var matType = this.Material.Type;
            if (matType == MaterialTypeDefOf.Soil)
                return ToolUseDefOf.Digging;
            else if (matType == MaterialTypeDefOf.Stone || matType == MaterialTypeDefOf.Metal)
                return ToolUseDefOf.Mining;
            throw new Exception();
        }

        protected override void AddSaveData(SaveTag tag)
        {
            this.AccumulatedWorkThisBreakStage.Save(tag, "WorkAppliedThisStage");
            this.AccumulatedWorkTotal.Save(tag, "WorkAppliedTotal");
        }
        public override void LoadData(SaveTag tag)
        {
            this.AccumulatedWorkThisBreakStage = (float)tag["WorkAppliedThisStage"].Value;
            this.AccumulatedWorkTotal = (float)tag["WorkAppliedTotal"].Value;

        }
        protected override void WriteExtra(IDataWriter w)
        {
            w.Write(this.AccumulatedWorkThisBreakStage);
            w.Write(this.AccumulatedWorkTotal);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.AccumulatedWorkThisBreakStage = r.ReadSingle();
            this.AccumulatedWorkTotal = r.ReadSingle();
        }
    }
}
