using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class LoadingBox : PictureBox
    {
        public Func<string> ProgressFunc;

        public LoadingBox()
        {
            int w = UIManager.DefaultLoadingSprite.Width, h = UIManager.DefaultLoadingSprite.Height;
            int ww = (int)Math.Sqrt(2 * w * w);
            int hh = (int)Math.Sqrt(2 * h * h);
            Sprite = UIManager.DefaultLoadingSprite;
            Width = ww;
            Height = hh;
            Origin = new Vector2(w, h) * 0.5f;
            Color = Color.LightSeaGreen;
            ClipToBounds = false;
            RotationFunc = () => (float)(4 * Math.PI * DateTime.Now.Millisecond / 2000f);
        }
        public override void OnBeforeDraw(SpriteBatch sb, Rectangle viewport)
        {
            sb.Draw(MapBase.Shadow, ScreenLocation, null, Color.White, 0, new Vector2(MapBase.Shadow.Bounds.Width, MapBase.Shadow.Bounds.Height)/2f, 2, SpriteEffects.None, 0);
        }
        public override void OnAfterDraw(SpriteBatch sb, Rectangle viewport)
        {
            UIManager.DrawStringOutlined(sb, ProgressFunc(), ScreenLocation, new Vector2(0.5f));
            UIManager.DrawStringOutlined(sb, TextFunc(), ScreenLocation + new Vector2(0, (int)(Dimensions.Y / 2f)), new Vector2(0.5f));
        }
    }
}
