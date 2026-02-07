using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Base;
using Project1.Core.Rendering;
using Project1.Core.Graphics;
using Project1.Core.Simulation;

namespace Project1.Core.Input.Tools.CellRendering
{
    public class BlockRenderer
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
        public void CreateMesh(Camera camera, IEnumerable<IntVec3> positions)
        {
            this.Batch.Clear();
            foreach (var pos in positions)
                camera.DrawBlockSelectionGlobal(this.Batch, this.BlockToken, pos);
        }
        public void DrawBlocks(MapBase map, Camera camera)
        {
            camera.PrepareShader(map);
            Coords.Iso(camera, 0 * Chunk.Size, 0 * Chunk.Size, 0, out float x, out float y);
            Coords.Rotate(camera, 0, 0, out int rotx, out int roty);
            var world = Matrix.CreateTranslation(new Vector3(x, y, (rotx + roty) * Chunk.Size));
            camera.Effect.Parameters["World"].SetValue(world);
            camera.Effect.CurrentTechnique.Passes["Pass1"].Apply();
            this.Batch.Draw();
        }
    }
}