using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    class BlockChair : Block
    {
        static AtlasDepthNormals.Node.Token[] Orientations = new AtlasDepthNormals.Node.Token[4];

        public BlockChair():base("Chair", opaque: false)
        {
            this.HidingAdjacent = false;
            Orientations[0] = Atlas.Load("blocks/furniture/chair", Block.HalfBlockDepthMap, Block.NormalMap);
            Orientations[1] = Atlas.Load("blocks/furniture/chair2", Block.HalfBlockDepthMap, Block.NormalMap);
            Orientations[2] = Atlas.Load("blocks/furniture/chairback2", Block.HalfBlockDepthMap, Block.NormalMap);
            Orientations[3] = Atlas.Load("blocks/furniture/chairback", Block.HalfBlockDepthMap, Block.NormalMap);
            this.BuildProperties.Category = ConstructionCategoryDefOf.Furniture;
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
        }
        protected override void Place(MapBase map, IntVec3 global, MaterialDef material, byte data, int variation, int orientation, bool notify = true)
        {
            base.Place(map, global, material, data, orientation, 0, notify);
        }
        public override float GetHeight(float x, float y)
        {
            return 0.5f;
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return Orientations[orientation];
        }
        
        public override AtlasDepthNormals.Node.Token GetDefault()
        {
            return Orientations[0];
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, Camera camera, Microsoft.Xna.Framework.Vector4 screenBounds, Microsoft.Xna.Framework.Color sunlight, Microsoft.Xna.Framework.Vector4 blocklight, Microsoft.Xna.Framework.Color fog, Microsoft.Xna.Framework.Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            DrawShadow(canvas.NonOpaque, global, camera, screenBounds, sunlight, blocklight, fog, tint, depth);
            return base.Draw(canvas, chunk, global, camera, screenBounds, sunlight, blocklight, fog, tint, depth, 0, variation, data, mat);
        }
    }
}
