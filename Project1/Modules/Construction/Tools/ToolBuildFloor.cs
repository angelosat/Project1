using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class ToolBuildFloor : ToolBlockBuild
    {
        public ToolBuildFloor()
        {
        }
        public ToolBuildFloor(Action<Args> callback)
            : base(callback)
        {
        }
        public override Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            this.Send(this.Begin, this.End, this.Orientation);
            this.Enabled = false;
            Sync();
            return Messages.Default;
        }
        public override void Update()
        {
            base.Update();
            if (!Enabled)
                return;
            if (this.Target == null)
                return;
            if (this.Target.Type != TargetType.Position)
                return;
            this.End = GetEnd(this.Begin, this.Target.Global);
        }
        static public IntVec3 GetEnd(IntVec3 begin, IntVec3 end)
        {
            var dx = end.X - begin.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - begin.Y;
            var ady = Math.Abs(dy);
            return begin + new IntVec3(adx, ady, 0);
        }
        protected override void DrawGrid(MySpriteBatch sb, MapBase map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            var box = this.Begin.GetBox(this.End)
                .Where(vec => this.Replacing ? map.GetBlock(vec) != BlockDefOf.Air : map.GetBlock(vec) == BlockDefOf.Air);
            cam.DrawCellHighlights(sb, Block.BlockBlueprint, box, color);
        }
       
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, Net.PlayerData player)
        {
            if (!this.Enabled)
                return;
            var targetArgs = player.Target;
            this.End = targetArgs.Type != TargetType.Null ? GetEnd(this.Begin, targetArgs.Global) : this.End;
            var box = this.Begin.GetBox(this.End)
                .Where(vec => this.Replacing ? map.GetBlock(vec) != BlockDefOf.Air : map.GetBlock(vec) == BlockDefOf.Air);

            camera.DrawCellHighlights(sb, Block.BlockBlueprint, box, Color.Red);
        }
    }
}
