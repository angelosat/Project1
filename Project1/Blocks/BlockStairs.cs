using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    class BlockStairs : Block
    {
        static Texture2D Depth1 = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/stairs/stairs1depth");
        static Texture2D Depth2 = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/stairs/stairs2depth");
        static Texture2D Depth3 = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/stairs/stairs3depth");
        static Texture2D Depth4 = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/stairs/stairs4depth");
        static Texture2D Normal1 = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/stairs/stairs1normal");
        static Texture2D Normal2 = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/stairs/stairs2normal");
        static Texture2D Normal3 = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/stairs/stairs3normal");
        static Texture2D Normal4 = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/stairs/stairs4normal");

        AtlasDepthNormals.Node.Token[] Parts = new AtlasDepthNormals.Node.Token[4];
        public BlockStairs()
            : base("Stairs", opaque: false)
        {
            this.HidingAdjacent = false;
            this.Parts[0] = Block.Atlas.Load("blocks/stairs/stairs1", Depth1, Normal1);
            this.Parts[1] = Block.Atlas.Load("blocks/stairs/stairs2", Depth2, Normal2);
            this.Parts[2] = Block.Atlas.Load("blocks/stairs/stairs3", Depth3, Normal3);
            this.Parts[3] = Block.Atlas.Load("blocks/stairs/stairs4", Depth4, Normal4);
            this.Variations.Add(this.Parts.First());
            this.BuildProperties.Category = ConstructionCategoryDefOf.Walls;
            this.Ingredient = new Ingredient().IsBuildingMaterial();
        }
        public override float GetHeight(byte data, float x, float y)
        {
            switch(data)
            {
                case 0:
                    return x <= .5f ? 1 : .5f;

                case 1:
                    return y <= .5f ? 1 : .5f;

                case 2:
                    return x > .5f ? 1 : .5f;

                case 3:
                    return y > .5f ? 1 : .5f;
               
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
