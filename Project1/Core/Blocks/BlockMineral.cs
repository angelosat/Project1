using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Graphics;
using Project1.Core.Graphics.Particles;
using Project1.Core.Simulation;
using Project1.Core.Systems.Materials;
using Project1.Framework.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Blocks
{
    class BlockMineral : Block
    {
        public override bool IsMinable => true;

        public override ParticleEmitterSphere GetEmitter()
        {
            return base.GetDustEmitter();
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            return Def.GetDefs<MaterialDef>().Where(mat => mat.Type == MaterialTypeDefOf.Stone || mat.Type == MaterialTypeDefOf.Metal);
        }
        public override void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            var token = this.Variations.First();
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, Color.White);
        }
        public override void DrawUI(SpriteBatch sb, Vector2 pos, byte state)
        {
            var token = this.Variations.First();
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, this.BlockState.GetTint(state));
        }

        public BlockMineral()
            : base("Mineral", 0, 1, true, true)
        {
            this.LoadVariations("stone5height19");
        }

        public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, Color tint, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenPos, sunlight, blocklight, cell.Material.Color.Multiply(tint), zoom, depth, cell);
        }

        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenBounds, sunlight, blocklight, fog, cell.Material.Color.Multiply(tint), zoom, depth, cell);
        }
        public override void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenBounds, sunlight, blocklight, fog, cell.Material.Color.Multiply(tint), zoom, depth, cell);
        }
    }
}
