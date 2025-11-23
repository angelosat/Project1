using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    class Borders
    {
        static public int Thickness = 0;//1;

        public static Texture2D GenerateOutline(Texture2D texture)
        {
            var gfx = Game1.Instance.GraphicsDevice;

            var sb = new SpriteBatch(gfx);

            var next = new RenderTarget2D(gfx, texture.Width + 2 * Thickness, texture.Height + 2 * Thickness);
            gfx.SetRenderTarget(next);
            gfx.Clear(Color.Transparent);

            Effect blur = Game1.Instance.Content.Load<Effect>("blur");
            blur.Parameters["Viewport"].SetValue(new Vector2(texture.Width + 2 * Thickness, texture.Height + 2 * Thickness));
            blur.CurrentTechnique = blur.Techniques["Technique1"];
            blur.CurrentTechnique.Passes["Pass1"].Apply();

            sb.Begin(0, null, null, null, null, blur);
            Vector2 tempVect;
            for (int d = 0; d < 8; d++)
            {
                double dir = d * (Math.PI / 4);
                tempVect = new Vector2((float)Math.Round(Math.Cos(dir)), -(float)Math.Round(Math.Sin(dir)));
                sb.Draw(texture, (Vector2.One - tempVect) * Thickness, Color.Red);
            }
            sb.End();
            blur.CurrentTechnique = blur.Techniques["Technique2"];
            blur.CurrentTechnique.Passes["Pass1"].Apply();
            sb.Begin(0, BlendState.Opaque, null, null, null, blur);
            sb.Draw(texture, Vector2.One * Thickness, Color.White);
            sb.End();

            gfx.SetRenderTarget(null);
            return next.ToTexture();
        }

        public static Texture2D GenerateDepthTexture(Texture2D texture, float layer = 0.5f) // default value is the object rests at the center of the block
        {
            var gfx = Game1.Instance.GraphicsDevice;

            SpriteBatch sb = new(gfx);

            var nextRender = new RenderTarget2D(gfx, texture.Width + 2 * Thickness, texture.Height + 2 * Thickness);
            gfx.SetRenderTarget(nextRender);
            gfx.Clear(Color.Transparent);
            Effect blur = Game1.Instance.Content.Load<Effect>("blur");
            blur.Parameters["Viewport"].SetValue(new Vector2(texture.Width + 2 * Thickness, texture.Height + 2 * Thickness));

            blur.CurrentTechnique = blur.Techniques["Solid"];
            blur.CurrentTechnique.Passes["Pass1"].Apply();
            sb.Begin(0, BlendState.Opaque, null, null, null, blur);

            sb.Draw(texture, Vector2.One * Thickness, new Color(layer, layer, layer, 1f));// Color.Gray); //gray because the billboard is at the center of the block
            sb.End();

            gfx.SetRenderTarget(null);
            return nextRender.ToTexture();
        }

        public static Texture2D GenerateDepthTexture(Texture2D texture, Texture2D mask)
        {
            if (mask == null)
                throw new ArgumentNullException();
            var gfx = Game1.Instance.GraphicsDevice;
    
            var sb = new SpriteBatch(gfx);

            var next = new RenderTarget2D(gfx, texture.Width + 2 * Thickness, texture.Height + 2 * Thickness);
            gfx.SetRenderTarget(next);
            gfx.Clear(Color.Transparent);

            Effect blur = Game1.Instance.Content.Load<Effect>("blur");
            blur.Parameters["Viewport"].SetValue(new Vector2(texture.Width + 2 * Thickness, texture.Height + 2 * Thickness));

            blur.CurrentTechnique = blur.Techniques["SpriteDepthTexture"];
            blur.CurrentTechnique.Passes["Pass1"].Apply();
            sb.Begin(0, BlendState.Opaque, null, null, null, blur);
            blur.Parameters["s"].SetValue(texture);
            blur.Parameters["s1"].SetValue(mask);

            sb.Draw(texture, Vector2.One * Thickness, Color.Gray); //gray because the billboard is at the center of the block
            sb.End();

            gfx.SetRenderTarget(null);
            return next.ToTexture();

        }

        public static Texture2D GenerateNormalTexture(Texture2D texture, Texture2D mask)
        {
            var gfx = Game1.Instance.GraphicsDevice;

            var sb = new SpriteBatch(gfx);

            var next = new RenderTarget2D(gfx, texture.Width, texture.Height);
            gfx.SetRenderTarget(next);
            gfx.Clear(Color.Transparent);

            Effect blur = Game1.Instance.Content.Load<Effect>("blur");
            blur.Parameters["Viewport"].SetValue(new Vector2(texture.Width, texture.Height));
          

            blur.CurrentTechnique = blur.Techniques["SpriteDepthTexture"];
            blur.CurrentTechnique.Passes["Pass1"].Apply();
            sb.Begin(0, BlendState.Opaque, null, null, null, blur);
            blur.Parameters["s"].SetValue(texture);
            blur.Parameters["s1"].SetValue(mask);
            sb.Draw(texture, Vector2.Zero, Color.Red);
            sb.End();

            gfx.SetRenderTarget(null);
            return next.ToTexture();
        }
    }
}
