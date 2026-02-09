using Microsoft.Xna.Framework;
using Project1.Core.Base;
using Project1.Core.Rendering;
using Project1.Core.Materials;
using Project1.Core.Simulation;
using Project1.Framework.Graphics;
using Project1.Framework;
using System;

namespace Project1.Core.Blocks
{
    class BlockConstruction : BlockWithEntity
    {
        public BlockConstruction()
            : base("Construction", solid: false, opaque: false)
        {
            this.HidingAdjacent = false;
            this.Variations.Add(Block.Atlas.Load("blocks/blockblueprint"));
        }
        public override bool IsStandableIn => false;
        internal override BlockEntity TryCreateNewBlockEntity(MapBase map, IntVec3 global, int orientation)
        {
            if (map.GetBlockEntity(global) is not BlockEntity existing || !existing.Comps.TryGetComp<BlockConstructionComp>(out _))
                throw new InvalidOperationException("Missing or unexpected block entity in construction block placement");
            return null;
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            var block = chunk.Map.GetBlockEntityComp<BlockConstructionComp>(global).Block;
            AtlasDepthNormals.Node.Token token;
                token = block.GetToken((int)camera.Rotation, chunk.Map.GetCell(global));

            var color = Color.White;
            var targetMesh = canvas.Opaque;
            return targetMesh.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, color, sunlight, blocklight, depth, this, global);
        }
    }
}
