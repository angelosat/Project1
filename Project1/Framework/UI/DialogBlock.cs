using Microsoft.Xna.Framework;
using Project1.Core.Base;

namespace Project1.Framework.UI
{
    public class DialogBlock : Control
    {
        public DialogBlock() { }
        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            e.Handled = true;
            return; 
        }
       
        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            e.Handled = true;
        }
        public override Rectangle BoundsScreen
        {
            get
            {
                return UIManager.Bounds;
            }
        }
        public override void BringToFront()
        {
            base.BringToFront();
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            sb.Draw(WindowManager.DimScreen, viewport, Color.Lerp(Color.White, Color.Transparent, 0.5f));
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.Draw(sb);
            sb.Draw(WindowManager.DimScreen, Game1.Instance.GraphicsDevice.Viewport.Bounds, Color.Lerp(Color.White, Color.Transparent, 0.5f));
        }
    }
}
