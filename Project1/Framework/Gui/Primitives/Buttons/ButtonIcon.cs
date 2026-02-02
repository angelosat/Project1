using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Start_a_Town_.UI
{
    class ButtonIcon : Button
    {
        Icon Icon;
        public ButtonIcon(Icon icon, Action action)
            : this(icon)
        {
            this.LeftClickAction = action;
        }
        public ButtonIcon(Icon icon)
        {
            this.Icon = icon;
            this.Width = UIManager.DefaultButtonHeight;
            this.Height = UIManager.DefaultButtonHeight;
        }
        public override void OnPaint(SpriteBatch sb)
        {
            DrawSprite(sb, this.Active, new Rectangle(0, 0, this.Width, this.Height),
               this.TexBackgroundColor,
               1,
               this.SprFx);
            this.Icon.Draw(sb, new Vector2(this.Width, this.Height) / 2, new Vector2(.5f));
        }
        static public void DrawSprite(SpriteBatch sb, bool active, Rectangle destRect, Color color, float opacity, SpriteEffects sprFx)
        {
            SpriteEffects fx = active ? sprFx : SpriteEffects.FlipVertically;
            Color c = Color.Lerp(Color.Transparent, color, opacity);
            sb.Draw(UIManager.DefaultButtonSprite, new Vector2(destRect.X, destRect.Y), Button.SpriteLeft, c, 0, Vector2.Zero, 1, fx, 0);
            sb.Draw(UIManager.DefaultButtonSprite,
                new Rectangle(
                    destRect.X + Button.SpriteLeft.Width,
                    destRect.Y,
                    destRect.Width - Button.SpriteLeft.Width - Button.SpriteRight.Width,
                    SpriteCenter.Height),
                Button.SpriteCenter, c, 0, Vector2.Zero, fx, 0);
            sb.Draw(UIManager.DefaultButtonSprite, new Vector2(destRect.X + destRect.Width - Button.SpriteRight.Width, destRect.Y), Button.SpriteRight, c, 0, Vector2.Zero, 1, fx, 0);
        }
    }
}
