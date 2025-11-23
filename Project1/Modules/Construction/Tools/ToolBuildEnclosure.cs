using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
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
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, Net.PlayerData player)
        {
            this.DrawGrid(sb, map, camera, Color.Red);
        }
    }
}
