using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Graphics;
using Project1.Core.Input.CellRendering;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Towns.Designations;
using Project1.Core.Towns.Tools;
using Project1.Framework;
using Project1.Framework.UI;
using System;
using System.Linq;

namespace Project1.Core.Towns.Digging
{
    class ToolDesignation : ToolDesignate3D
    {
        internal DesignationDef DesignationDef;
        readonly BlockRenderer Renderer = new();
        IntVec3 PrevEnd;
        public ToolDesignation()
        {

        }
        public override Icon GetIcon()
        {
            return Icon.Construction;
        }
        public ToolDesignation(Action<IntVec3, IntVec3, bool> callback)
        {
            this.Callback = callback;
        }
        /// <summary>
        /// TODO optimize
        /// </summary>
        /// <param name="map"></param>
        /// <param name="camera"></param>
        protected void Validate(RenderContext ctx)
        {
            var map = ctx.Map;
            var positions = this.Begin.GetBox(this.End)
                .Where(v => map.GetBlock(v) != BlockDefOf.Air.Block || map.IsUndiscovered(v));
            this.Renderer.CreateMesh(ctx, positions);
        }
        public override void UpdateRemote(InteractionTarget target)
        {
            if(target.Type == TargetType.Cell)
            this.End = target.Global;
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, RenderContext ctx)
        {
            if (!this.Enabled)
                return;
            var viewport = ctx.MapViewport;
            if (this.End != this.PrevEnd)
            {
                this.Validate(ctx);
                this.PrevEnd = this.End;
            }
            this.Renderer.DrawBlocks(ctx);
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, RenderContext ctx, PlayerData player)
        {
            if (!this.Enabled)
                return;
            var map = ctx.Map;
            var positions = this.Begin.GetBox(this.End)
                .Where(v => map.GetBlock(v) != BlockDefOf.Air.Block);
            ctx.Renderer.DrawCellHighlights(sb, Block.BlockBlueprint, positions, Color.Red);
        }
    }
}
