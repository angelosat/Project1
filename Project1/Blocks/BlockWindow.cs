using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    class BlockWindow : Block
    {
        AtlasDepthNormals.Node.Token[][][] PartsSeparate;
        
        public BlockWindow()
            : base("Window", opaque: false)
        {
            this.HidingAdjacent = false;
            this.PartsSeparate = 
                new AtlasDepthNormals.Node.Token[][][] { 
                    new AtlasDepthNormals.Node.Token[][]{
                        new AtlasDepthNormals.Node.Token[]{
                            Atlas.Load("blocks/windows/windowframebottom", BlockDepthMap, NormalMap),
                            Atlas.Load("blocks/windows/glassbottom", BlockDepthMap, NormalMap)},
                        new AtlasDepthNormals.Node.Token[]{
                            Atlas.Load("blocks/windows/windowframebottom2", BlockDepthMap, NormalMap),
                            Atlas.Load("blocks/windows/glassbottom2", BlockDepthMap, NormalMap)}},
                    new AtlasDepthNormals.Node.Token[][]{
                        new AtlasDepthNormals.Node.Token[]{
                            Atlas.Load("blocks/windows/windowframetop", BlockDepthMap, NormalMap),
                            Atlas.Load("blocks/windows/glasstop", BlockDepthMap, NormalMap)},
                        new AtlasDepthNormals.Node.Token[]{
                            Atlas.Load("blocks/windows/windowframetop2", BlockDepthMap, NormalMap),
                            Atlas.Load("blocks/windows/glasstop2", BlockDepthMap, NormalMap)}}
            };

            this.Variations.Add(this.PartsSeparate.First().First().First());
            this.BuildProperties.Category = ConstructionCategoryDefOf.Doors;
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
            this.Size = new(1, 1, 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part">0: bottom part, 1: top part</param>
        /// <returns></returns>
        static byte GetData(int part)
        {
            if (part != 0 && part != 1)
                throw new ArgumentException();
            return (byte)part;
        }

        static int GetPartIndex(byte data)
        {
            return data;
        }

        protected override void Place(MapBase map, IntVec3 global, MaterialDef material, byte data, int variation, int orientation, bool notify = true)
        {
            base.Place(map, global, material, GetData(0), variation, orientation);
            base.Place(map, global + IntVec3.UnitZ, material, GetData(1), variation, orientation, notify);
        }
     
        public override void DrawPreview(MySpriteBatch sb, MapBase map, IntVec3 global, Camera cam, byte data, MaterialDef material, int variation = 0, int orientation = 0)
        {
            var orientationindex = (int)(orientation + cam.Rotation) % 2;
            var bottom = this.PartsSeparate[0][orientationindex][0];
            var top = this.PartsSeparate[1][orientationindex][0];
            sb.DrawBlock(Block.Atlas.Texture, map, global, bottom, cam, Color.Transparent, Color.White * 0.5f, Color.White, Vector4.One);
            sb.DrawBlock(Block.Atlas.Texture, map, global + IntVec3.UnitZ, top, cam, Color.Transparent, Color.White * 0.5f, Color.White, Vector4.One);
        }

        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            //15 18 65
            //sunlight 255 16 239(159?) 255
            //blocklight .0625 1

            //15 20 65
            //sunlight 255 16 239(191?) 255
            //blocklight .0625 1

            //15 18 66
            //sunlight 255 16 16 255
            //blocklight .0625 1

            //15 20 66
            //sunlight255 16 16 255
            //blocklight .0625 1

            var partindex = GetPartIndex(data);
            var orientationindex = (int)(orientation + camera.Rotation) % 2;
            var parts = this.PartsSeparate[partindex][orientationindex];
            var frame = parts[0];
            var glass = parts[1];
            canvas.Opaque.DrawBlock(Block.Atlas.Texture, screenBounds, frame, camera.Zoom, fog, Color.White, sunlight, blocklight, depth, this, global);
            return canvas.Transparent.DrawBlock(Block.Atlas.Texture, screenBounds, glass, camera.Zoom, fog, Color.White, sunlight, blocklight, depth, this, global);
        }
    }
}
