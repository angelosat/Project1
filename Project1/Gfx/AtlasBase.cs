using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    public abstract class AtlasBase : Inspectable
    {
        public Texture2D Texture;
        public Texture2D DepthTexture;
        internal void Begin(Effect fx, MySpriteBatch sb)
        {
            if (fx.Parameters["s"].GetValueTexture2D() == this.Texture)
                sb.Flush();
            this.Begin(fx);
        }
        internal void Begin(Effect fx)
        {
            fx.Parameters["s"].SetValue(this.Texture);
            fx.Parameters["s1"].SetValue(this.DepthTexture);
            fx.CurrentTechnique.Passes["Pass1"].Apply(); // since porting to monogame, need to apply the pass after setting the textures on the shader samples
        }
    }
}
