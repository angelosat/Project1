using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Framework.UI;

namespace Start_a_Town_.UI
{
    class ColoredRectangle : ButtonBase
    {
        public ColoredRectangle(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            this.DrawHighlight(sb, this.BoundsScreen, this.Tint);
        }
    }
}
