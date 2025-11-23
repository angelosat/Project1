using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Tooltip : Control
    {
        Vector2 Offset = new(16);

        public Tooltip(ITooltippable obj)
        {
            this.Tag = obj;
            ClientLocation = new Vector2(UIManager.BorderPx);
            AutoSize = true;
            MouseThrough = true;
        }

        public override void OnMouseEnter()
        {

        }

        public override bool HitTest()
        {
            return false;
        }

        public override Vector2 ScreenLocation
        {
            get
            {
                if (TooltipManager.MouseTooltips)
                    return new Vector2(Math.Max(Math.Min(Controller.Instance.msCurrent.X / UIManager.Scale + Offset.X, UIManager.Width - Width), 0), Math.Max(Math.Min(Controller.Instance.msCurrent.Y / UIManager.Scale + Offset.Y, UIManager.Height - Height), 0));
                else
                    return new Vector2(UIManager.Width - Width - UIManager.BorderPx, UIManager.Height - Height - UIManager.BorderPx);
            }
        }

        public override void OnPaint(SpriteBatch sb)
        {
            BackgroundStyle.Tooltip.Draw(sb, Size, Color, 1);
        }
        
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb, this.BoundsScreen);
        }

        protected override void OnClientSizeChanged()
        {
            Size = new Rectangle(0, 0, ClientSize.Width + 2 * UIManager.BorderPx, ClientSize.Height + 2 * UIManager.BorderPx);
            base.OnClientSizeChanged();
        }
    }
}
