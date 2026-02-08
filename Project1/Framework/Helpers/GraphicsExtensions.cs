using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project1.Framework.Helpers
{
    static internal class GraphicsExtensions
    {
        public static Texture2D ToTexture(this RenderTarget2D render)
        {
            var data = new Color[render.Width * render.Height];
            render.GetData(data);
            var texture = new Texture2D(render.GraphicsDevice, render.Width, render.Height);
            texture.SetData(data);
            return texture;
        }
    }
}
