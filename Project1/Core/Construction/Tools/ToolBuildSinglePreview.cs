using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Graphics;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Rendering;
using Project1.Core.Serialization;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Project1.Core.Construction.Tools;

class ToolBuildSinglePreview : ToolBlockBuild
{
    public override string Status => "Select location";

    public ToolBuildSinglePreview()
    {

    }
    public ToolBuildSinglePreview(Action<Args> callback)
        : base(callback)
    {

    }
    public ToolBuildSinglePreview(Action<Args> callback, Func<Block> blockGetter)
        : this(callback)
    {
        this.Block = blockGetter();
    }
    
    public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
    {
        if (!this.Enabled)
            return Messages.Default;
        if (this.Target == null)
            return Messages.Default;
        this.Enabled = false;

        if(this.Validate(true))
            this.Send(this.Begin, this.Begin, this.Orientation);

        return Messages.Default;
    }
    bool Validate(bool notify)
    {
        var map = this.Target.Map;
        var interactionCells = this.Block.GetReservedInteractionCells(this.Begin, this.Orientation);
        if (!interactionCells.All(c => map.Contains(c) && !map.IsSolid(c)))
        {
            if(notify)
                Log.Warning("Interaction spots blocked.");
            return false;
        }
        return true;
    }
    protected override void DrawGrid(MySpriteBatch sb, RenderContext ctx, Color color)
    {
        if (!this.Enabled)
            return;
        ctx.Renderer.DrawGridCell(sb, this.Valid ? Color.Lime : Color.Red, this.Begin);
    }
    static public List<Vector3> GetPositions(Vector3 a, Vector3 b)
    {
        return new List<Vector3>() { a };
    }
    internal override void DrawAfterWorld(MySpriteBatch sb, RenderContext ctx)
    {
        var cam = ctx.Camera;
        var map = ctx.Map;
        var view = ctx.View;
        var renderer = ctx.Renderer;
        if (this.Target == null && !this.Enabled)
            return;

        var atlastoken = this.Block.GetDefault();
        var global = this.Enabled ? this.Begin : (IntVec3)this.Target.FaceGlobal;
        atlastoken.Atlas.Begin(Renderer.Effect);
        this.Block.DrawPreview(sb, global, view, this.State, this.Material, this.Variation, this.Orientation);
        sb.Flush();

        this.Block.DrawInteractionCells(sb, ctx, global, this.Orientation);
    }
    internal override void DrawAfterWorldRemote(MySpriteBatch sb, RenderContext ctx, PlayerData player)
    {
        this.DrawAfterWorld(sb, ctx);
    }

    protected override void WriteData(IDataWriter w)
    {
        w.Write(this.Block.BlockDef);
    }
    protected override void ReadData(IDataReader r)
    {
        this.Block = r.ReadDef<BlockDef>().Block;
    }
    internal override void RotateAntiClockwise()
    {
        this.Orientation -= 1;
        if (this.Orientation < 0)
            this.Orientation = 3;
    }

    internal override void RotateClockwise()
    {
        this.Orientation = (this.Orientation + 1) % 4;
    }
}
