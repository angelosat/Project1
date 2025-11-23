using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Towns;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class ToolBlockErase : ToolSelect3D
    {
        public override Icon GetIcon()
        {
            return Icon.Cross;
        }
        ControlTool PreviousTool;
        public ToolBlockErase()
        {

        }
        public ToolBlockErase(ControlTool previousTool)
        {
            this.Add = RemoveZone;
            this.PreviousTool = previousTool;
        }
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyValue == 17)
                ToolManager.SetTool(this.PreviousTool);
        }
        private static void RemoveZone(Vector3 min, Vector3 max, bool remove)
        {
            //var a = new ToolBlockBuildOld.Args(ToolBlockBuildOld.Modes.Box, min, max, true, InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu), false, 0);
            var a = new ToolBlockBuild.Args(BuildToolDefOf.Box, min, max, true, InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu), false, 0);
            PacketDesignateConstruction.Send(Client.Instance, a);
        }
       
        internal override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
            if (!this.Enabled)
                return;
            var positions = this.Begin.GetBox(this.End)
                .Where(v => map.GetBlock(v) != BlockDefOf.Air);
            camera.DrawCellHighlights(sb, Block.BlockBlueprint, positions, Color.Red);
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, PlayerData player)
        {
            if (!this.Enabled)
                return;
            var positions = this.Begin.GetBox(this.End)
                .Where(v => map.GetBlock(v) != BlockDefOf.Air);
            camera.DrawCellHighlights(sb, Block.BlockBlueprint, positions, Color.Red);
        }
    }
}
