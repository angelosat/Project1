using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Base;
using Project1.Core.Materials;
using Project1.Core.Simulation;
using Project1.Framework.Graphics;
using Project1.Framework;
using Project1.Core.Graphics;

namespace Project1.Core
{
    class BlockFluid : Block
    {
        AtlasDepthNormals.Node.Token[][] Assets;
        enum Fullness { Half, Full };
       
        public BlockFluid()
            : base("Water", opaque: false, density: 0.2f, solid: false)
        {
            this.HidingAdjacent = false;
            this.LoadVariations("water/water1", "water/water2", "water/water3", "water/water4");
            this.Assets = new AtlasDepthNormals.Node.Token[2][];
            this.Assets[(int)Fullness.Half] = [
                Block.Atlas.Load("blocks/water/water1half", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap)
            ];
            this.Assets[(int)Fullness.Full] = [
                Block.Atlas.Load("blocks/water/water1")
            ];
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            yield return MaterialDefOf.Water;
        }
        
        public override BlockEntity GetBlockEntityOrNew(MapBase map, IntVec3 originGlobal, BlockComp.Spec args)
        {
            return new BlockFluidEntity(this.BlockDef, originGlobal);
        }
        public override void OnNeighborChanged(MapBase map, IntVec3 global, IntVec3 source)
        {
            return;
        }

        public override bool IsTargetable(Vector3 global)
        {
            return false;
        }
        public override float GetHeight(byte data, float x, float y)
        {
            return data == 1 ? 1 : .5f; // if full (1) return 1 height, else return .5f height for half fullness (0)
        }
        public override float GetDensity(byte data, Vector3 global)
        {
            return data == 1 ? this.Density : 0;
        }
        /// <summary>
        /// 0 is halfblock, 1 is full
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        static public byte GetData(int depth)
        {
            return (byte)depth;
        }
        public override MyVertex[] Draw(Chunk chunk, IntVec3 global, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            return chunk.Canvas.Transparent.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][0], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth, this, global);
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            return canvas.Transparent.DrawBlock(Block.Atlas.Texture, 
                screenBounds, 
                this.Assets[data][0], 
                camera.Zoom, 
                fog, 
                tint, 
                Color.White, 
                new Color(sunlight.R, sunlight.G, sunlight.A, sunlight.A), 
                blocklight, 
                Color.Red.ToVector4(), 
                depth, 
                this, 
                global);
        }
    }
}
