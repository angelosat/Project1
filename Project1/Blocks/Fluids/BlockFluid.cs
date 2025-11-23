using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
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
            this.Assets[(int)Fullness.Half] = new AtlasDepthNormals.Node.Token[1]{
                Block.Atlas.Load("blocks/water/water1half", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap)
            };
            this.Assets[(int)Fullness.Full] = new AtlasDepthNormals.Node.Token[1]{
                Block.Atlas.Load("blocks/water/water1")
            };
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            yield return MaterialDefOf.Water;
        }
        
        public override BlockEntity CreateBlockEntity(IntVec3 originGlobal)
        {
            return new BlockFluidEntity(originGlobal);
        }
        public override void NeighborChanged(MapBase map, IntVec3 global)
        {
            map.AddBlockEntity(global, new BlockFluidEntity(global));
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

        //public override MyVertex[] Draw(MySpriteBatch sb, Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        //{
        //    return camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][0], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth, this, blockcoords);
        //}
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
