using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Graphics;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Construction.Tools
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
      
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, PlayerData player)
        {
            this.DrawGrid(sb, map, camera, Color.Red);
        }
        protected override void WriteData(IDataWriter w)
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
