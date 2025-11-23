using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
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
