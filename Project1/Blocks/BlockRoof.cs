using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;
using System;
using System.Linq;

namespace Start_a_Town_
{
    class BlockRoof : Block
    {
        static readonly Texture2D Depth1 = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/roof/depth1height19");
        static readonly Texture2D Depth2 = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/roof/depth2height19");
      
        readonly AtlasDepthNormals.Node.Token[] Parts = new AtlasDepthNormals.Node.Token[4];
        public BlockRoof() 
            : base("Roof", opaque: false)
        {
            this.HidingAdjacent = false;
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
            this.Parts[0] = Atlas.Load("blocks/roof/roof1height19", Depth1, NormalMap);
            this.Parts[3] = Atlas.Load("blocks/roof/roof2height19", Depth2, NormalMap);
            this.Parts[2] = Atlas.Load("blocks/roof/roof3height19", BlockDepthMap, NormalMap);
            this.Parts[1] = Atlas.Load("blocks/roof/roof4height19", BlockDepthMap, NormalMap);
            this.Variations.Add(this.Parts.First());
            this.BuildProperties.Category = ConstructionCategoryDefOf.Walls;
        }
        public override float GetHeight(byte data, float x, float y)
        {
            switch (data)
            {
                case 0:
                    return (1 - y);

                case 1:
                    return (1 - x);

                case 2:
                    return y;

                case 3:
                    return x;

                default:
                    break;
            }
            return 1;
        }
        public override float GetHeight(float x, float y)
        {
            throw new Exception();
        }
        static byte GetData(int rotation)
        {
            return (byte)rotation;
        }

        public override AtlasDepthNormals.Node.Token GetPreviewToken(int variation, int orientation, int cameraRotation, byte data)
        {
            var o = (orientation + cameraRotation) % 4;
            return this.Parts[o];
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            var o = (data + cameraRotation) % 4;
            return this.Parts[o];
        }
        protected override void Place(MapBase map, IntVec3 global, MaterialDef material, byte data, int variation, int orientation, bool notify = true)
        {
            base.Place(map, global, material, GetData(orientation), variation, orientation, notify);
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            if (this == BlockDefOf.Air)
                return null;
            var material = mat.ColorVector;// this.GetColorVector(data);
            var token = this.GetToken(variation, orientation, (int)camera.Rotation, data);// maybe change the method to accept double so i don't have to cast the camera rotation to int?
            return canvas.Opaque.DrawBlock(Block.Atlas.Texture, screenBounds,
                token,
                camera.Zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth, this, global);
        }
    }
}
