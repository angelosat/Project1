using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class BlockWoodenDeck : Block
    {
        public BlockWoodenDeck()
         : base("WoodenDeck", 0, 1, true, true)
        {
            this.GrayScale = Block.Atlas.Load("blocks/woodvertical");
            this.Variations.Add(this.GrayScale);
            this.Ingredient = new Ingredient(RawMaterialDefOf.Planks, null, null, 1);// 4);
            this.BuildProperties.Complexity = 2;
            this.BuildProperties.Dimension = 4;
            this.BuildProperties.Category = ConstructionCategoryDefOf.Walls;
        }

        public override Particles.ParticleEmitterSphere GetEmitter()
        {
            return base.GetDustEmitter();
        }

        public override void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            var token = this.Variations.First();
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, Color.White);
        }
        public override void DrawUI(SpriteBatch sb, Vector2 pos, byte state)
        {
            var token = this.GrayScale;
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, this.BlockState.GetTint(state));
        }

        public override bool IsDeconstructible => true;

        readonly AtlasDepthNormals.Node.Token GrayScale;

     

        public override LootTable GetLootTable(byte data)
        {
            var table =
                new LootTable(
                    new Loot(() => ItemFactory.CreateFrom(RawMaterialDefOf.Planks, MaterialDefOf.Human))// this.GetMaterial(data)))
                    );
            return table;
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            return Def.GetDefs<MaterialDef>().Where(mat => mat.Type == MaterialTypeDefOf.Wood);
        }
       
        public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, Color tint, float zoom, float depth, Cell cell)
        {
            var sourceRect = this.GrayScale.Rectangle;
            var mat = cell.Material;
            tint = tint.Multiply(mat.Color);
            sb.DrawBlock(Block.Atlas.Texture, screenPos, this.GrayScale.Rectangle, zoom, tint, sunlight, blocklight, depth);
        }

        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            var mat = cell.Material;
            var finaltint = mat.Color;
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.GrayScale, zoom, fog, finaltint.Multiply(tint), sunlight, blocklight, depth);
        }
        public override void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            var mat = cell.Material;
            var finaltint = mat.Color;
            finaltint = finaltint.Multiply(tint) * .66f;
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.GrayScale, zoom, fog, finaltint.Multiply(tint), sunlight, blocklight, depth, this);
        }

        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraOrientation, byte data)
        {
            return this.GrayScale;
        }
    }
}
