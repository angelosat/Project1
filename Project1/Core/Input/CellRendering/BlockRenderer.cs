using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Graphics;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Graphics;
using System.Collections.Generic;

namespace Project1.Core.Input.CellRendering;

public sealed class BlockRenderer
{
    readonly AtlasDepthNormals.Node.Token BlockToken = Block.BlockBlueprint;
    readonly MySpriteBatch Batch = new(Game1.Instance.GraphicsDevice);
    public BlockRenderer()
    {

    }
    public BlockRenderer(AtlasDepthNormals.Node.Token textureToken)
    {
        this.BlockToken = textureToken;
    }
    public void CreateMesh(RenderContext ctx, IEnumerable<IntVec3> positions)
    {
        this.Batch.Clear();
        var r = ctx.Renderer;
        var c = ctx.Camera;
        var view = ctx.View;
        foreach (var pos in positions)
            r.DrawBlockSelectionGlobal(this.Batch, view, this.BlockToken, pos);
    }
    public void DrawBlocks(RenderContext ctx)
    {
        var camera = ctx.Camera;
        var renderer = ctx.Renderer;
        var view = ctx.View;
        renderer.PrepareShader(ctx.View);
        view.Iso(0 * Chunk.Size, 0 * Chunk.Size, 0, out float x, out float y);
        //Coords.Rotate(camera, 0, 0, out int rotx, out int roty);
        view.Rotate(0, 0, out int rotx, out int roty);
        var world = Matrix.CreateTranslation(new Vector3(x, y, (rotx + roty) * Chunk.Size));
        renderer.Effect.Parameters["World"].SetValue(world);
        renderer.Effect.CurrentTechnique.Passes["Pass1"].Apply();
        this.Batch.Draw();
    }
}