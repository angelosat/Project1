using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Construction.Packets;
using Project1.Core.Graphics;
using Project1.Core.Input;
using Project1.Core.Input.Building;
using Project1.Core.Networking;
using Project1.Core.Rendering;
using Project1.Core.Screens;
using Project1.Core.Towns.Tools;
using Project1.Framework;
using Project1.Framework.Input;
using Project1.Framework.UI;
using System.Linq;

namespace Project1.Core.Construction.Tools;

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
    private static void RemoveZone(MapId mapid, Vector3 min, Vector3 max, bool remove)
    {
        var a = new ToolBlockBuild.Args(BuildToolDefOf.Box, mapid, min, max, true, InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu), false, 0);
        PacketDesignateConstruction.SendRemove(Client.Instance, a);
    }
   
    internal override void DrawBeforeWorld(MySpriteBatch sb, RenderContext ctx)
    {
        if (!this.Enabled)
            return;
        var map = ctx.Map;
        var positions = this.Begin.GetBox(this.End)
            .Where(v => map.GetBlock(v) != BlockDefOf.Air.Block);
        positions = this.Begin.GetBoxHollow(this.End);
        ctx.Renderer.DrawCellHighlights(sb, Block.BlockBlueprint, positions, Color.Red);
    }
    internal override void DrawAfterWorldRemote(MySpriteBatch sb, RenderContext ctx, PlayerData player)
    {
        if (!this.Enabled)
            return;
        var positions = this.Begin.GetBox(this.End)
            .Where(v => ctx.Map.GetBlock(v) != BlockDefOf.Air.Block);
        ctx.Renderer.DrawCellHighlights(sb, Block.BlockBlueprint, positions, Color.Red);
    }
}
