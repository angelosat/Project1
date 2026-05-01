using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Graphics;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Graphics;
using Project1.Framework.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input.CellRendering;

public sealed class BlockRendererNew
{
    readonly Dictionary<int, MySpriteBatch> Slices = new();
    readonly AtlasDepthNormals.Node.Token BlockToken = Block.BlockBlueprint;
    bool Validated;
    public BlockRendererNew()
    {

    }
    public BlockRendererNew(AtlasDepthNormals.Node.Token blockTexture)
    {
        this.BlockToken = blockTexture;
    }
    public void CreateMesh(RenderContext ctx, IEnumerable<IntVec3> positions)
    {
        if (this.Validated)
            return;
        this.Validated = true;
        this.Slices.Clear();
        var view = ctx.View;
        foreach (var cells in positions.GroupBy(g => g.Z))
        {
            foreach (var cell in cells)
                ctx.Renderer.DrawBlockSelectionGlobal(
                    this.Slices.GetOrAdd(cells.Key, sliceCtor),
                    view,
                    this.BlockToken,
                    cell);
        }

        static MySpriteBatch sliceCtor()
        {
            return new(Game1.Instance.GraphicsDevice);
        }
    }
    public void DrawBlocks(RenderContext ctx, IEnumerable<IntVec3> positions)
    {
        var view = ctx.View;
        var renderer = ctx.Renderer;
        this.CreateMesh(ctx, positions);
        renderer.PrepareShader(ctx.View);
        //camera.PrepareShaderTransparent(map);
        view.Rotate(0, 0, out int rotx, out int roty);
        var world = Matrix.CreateTranslation(new Vector3(0, 0, (rotx + roty) * Chunk.Size));
        renderer.Effect.Parameters["World"].SetValue(world);
        renderer.Effect.CurrentTechnique.Passes["Pass1"].Apply();
        this.BlockToken.Atlas.Begin(renderer.Effect);
        foreach (var slice in this.Slices)
            if (slice.Key <= ctx.View.Settings.DrawLevel)
                slice.Value.Draw();
    }

    internal void Invalidate()
    {
        this.Validated = false;
        this.Slices.Clear();
    }
    //internal void MarkDirty(IntVec3 position)
    //{
    //    this._dirtyZ.Add(position.Z);
    //}
    //HashSet<int> _dirtyZ = [];
}