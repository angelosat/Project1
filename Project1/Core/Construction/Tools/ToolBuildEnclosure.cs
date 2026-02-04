using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Framework.Blocks;
using Project1.Framework.Base;
using Project1.Framework.Net;
using Project1.Framework.Rendering;
using Project1.Framework.WorldGen;
using Start_a_Town_;

namespace Project1.Core.Construction.Tools
{
    class ToolBuildEnclosure : ToolBuildBox
    {
        public ToolBuildEnclosure()
        {
        }
        public ToolBuildEnclosure(Action<Args> callback)
            : base(callback)
        {
        }
        
        protected override void DrawGrid(MySpriteBatch sb, MapBase map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            var end = this.End + IntVec3.UnitZ * this.Height;
            var box = this.Begin.GetBox(end);
            if (Math.Abs(this.End.X - this.Begin.X) > 1 && Math.Abs(this.End.Y - this.Begin.Y) > 1)
            {
                VectorHelper.GetMinMaxVector3(this.Begin, end, out IntVec3 a, out IntVec3 b);
                var boxInner = (a + new IntVec3(1, 1, 0)).GetBox(b - new IntVec3(1, 1, 0));
                box = box.Except(boxInner).ToList();
            }
            cam.DrawCellHighlights(sb, Block.BlockBlueprint, box, color);
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, PlayerData player)
        {
            this.DrawGrid(sb, map, camera, Color.Red);
        }
    }
}
