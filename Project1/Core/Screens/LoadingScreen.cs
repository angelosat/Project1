using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Framework.UI;

namespace Project1.Core.Screens
{
    class LoadingScreen
    {
        static public void Draw(SpriteBatch sb, string message)
        {
            sb.Begin();
            UIManager.DrawStringOutlined(sb, message, new Vector2(Game1.Instance.GraphicsDevice.Viewport.Width / 2, 3 * Game1.Instance.GraphicsDevice.Viewport.Height / 4), Alignment.Horizontal.Center);
            sb.End();
        }
    }
}
