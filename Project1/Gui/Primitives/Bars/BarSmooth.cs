using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class BarSmooth : ButtonBase
    {
        public bool Invert;
        public override Texture2D BackgroundTexture
        {
            get
            {
                return UIManager.DefaultProgressBar;
            }
            set
            {
            }
        }

        public IProgressBar Object { get; set; }

        public float Percentage => (Object != null) ? this.Object.Percentage : 0;
        float SmoothPercentage;

        public BarSmooth()
        {
            this.Height = UIManager.DefaultProgressBarStrip.Bounds.Height;
            this.Width = 100;
            this.BackgroundColor = Color.Black * 0.5f;
        }
        public BarSmooth(IProgressBar progress)
            : this()
        {
            this.Object = progress;
            this.SmoothPercentage = progress.Percentage;
        }
       
        public void Draw(SpriteBatch sb, Rectangle viewport, Vector2 screenLoc, int width, float scale)
        {
            base.Draw(sb, viewport);
            var d = this.Percentage - this.SmoothPercentage;
            this.SmoothPercentage += d*.05f;
            var percentage = this.Percentage;
            var w = (int)(width * scale);
            var h = (int)(UIManager.DefaultProgressBar.Height * scale);
            var x = (int)screenLoc.X - w / 2;
            var y = (int)screenLoc.Y - h / 2;
            var rect = new Rectangle(x, y, w, h);
            rect.DrawHighlight(sb, Color.Black * .5f);
            var vec = new Vector2(x, y);
            // TODO: OPTIMIZE: create a texture for the static graphics and only draw the smooth bar dynamically
            sb.Draw(UIManager.DefaultProgressBar, vec, new Rectangle(0, 0, (int)(w * percentage), h), Color.Orange * 0.5f, 0, Vector2.Zero, Vector2.One, SpriteEffects.FlipVertically, 0);
            sb.Draw(UIManager.DefaultProgressBar, vec, new Rectangle(0, 0, (int)(w * this.SmoothPercentage), h), Color.Orange);
            if (width == InteractionBar.DefaultWidth)
                sb.Draw(UIManager.ProgressBarBorder, vec, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
