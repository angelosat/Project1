using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Base;
using Project1.Core.Input.Tools.CellRendering;
using Project1.Core.Net;
using Project1.Core.UI;
using Project1.Core.Rendering;
using Project1.Core.Towns.Tools;
using Project1.Core.Towns.Designations;
using Project1.Core;
using Project1.Core.Helpers;
using Project1.Core.Simulation;

namespace Project1.Core.Towns.Digging
{
    class ToolDigging : ToolDesignate3D
    {
        internal DesignationDef DesignationDef;
        readonly BlockRenderer Renderer = new();
        IntVec3 PrevEnd;
        public ToolDigging()
        {

        }
        public override Icon GetIcon()
        {
            return Icon.Construction;
        }
        public ToolDigging(Action<IntVec3, IntVec3, bool> callback)
        {
            this.Callback = callback;
        }
        /// <summary>
        /// TODO optimize
        /// </summary>
        /// <param name="map"></param>
        /// <param name="camera"></param>
        protected void Validate(MapBase map, Camera camera)
        {
            var positions = this.Begin.GetBox(this.End)
                .Where(v => map.GetBlock(v) != BlockDefOf.Air.Worker || map.IsUndiscovered(v));
            this.Renderer.CreateMesh(camera, positions);
        }
        public override void UpdateRemote(TargetArgs target)
        {
            if(target.Type == TargetType.Position)
            this.End = target.Global;
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
            if (!this.Enabled)
                return;

            if (this.End != this.PrevEnd)
            {
                this.Validate(map, camera);
                this.PrevEnd = this.End;
            }
            this.Renderer.DrawBlocks(map, camera);
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, PlayerData player)
        {
            if (!this.Enabled)
                return;
            var positions = this.Begin.GetBox(this.End)
                .Where(v => map.GetBlock(v) != BlockDefOf.Air.Worker);
            camera.DrawCellHighlights(sb, Block.BlockBlueprint, positions, Color.Red);
        }
    }
}
