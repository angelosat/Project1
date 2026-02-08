using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Framework.UI;

namespace Project1.Core
{
    class NameplatesContainer : Control
    {
        public override Rectangle BoundsScreen => UIManager.Bounds;
        public override int Width { get => BoundsScreen.Width; }
        public override int Height { get => BoundsScreen.Height; }

        public NameplatesContainer()
        {
            this.MouseThrough = true;
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, UIManager.Bounds);
        }
    }
}
