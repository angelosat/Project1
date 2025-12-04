using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class ToolBuildBox : ToolBuildWithHeight
    {
        public ToolBuildBox()
        {

        }
        public ToolBuildBox(Action<Args> callback)
            : base(callback)
        {
        }
        
        protected override void DrawGrid(MySpriteBatch sb, MapBase map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            var end = this.End + IntVec3.UnitZ * this.Height;

            var box = this.Begin.GetBox(end);

            cam.DrawCellHighlights(sb, Block.BlockBlueprint, box, color);
        }
      
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, Net.PlayerData player)
        {
            this.DrawGrid(sb, map, camera, Color.Red);
        }
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            base.WriteData(w);
            w.Write(this.SettingHeight);
            w.Write(this.Height);
        }
        protected override void ReadData(IDataReader r)
        {
            base.ReadData(r);
            this.SettingHeight = r.ReadBoolean();
            this.Height = r.ReadInt32();
        }
    }
}
