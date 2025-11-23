using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public class FloatingBar : InteractionBar
    {
        static public float Speed = 0.5f, Life = 300f;
        float CurrentLife;
       
        public override void Update()
        {
            Rectangle rect = Ingame.Instance.Camera.GetScreenBounds(Object.Global, Object.GetComponent<SpriteComponent>("Sprite").Sprite.GetBounds());
            Location = new Vector2(rect.X + rect.Width / 2 - Width / 2, rect.Y);
            CurrentLife -= 1;
            if (CurrentLife < 0)
            {
                Parent.Controls.Remove(this);
            }
            base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Object is null)
                return;

            Color alpha = Color.Lerp(Color.Transparent, Color.White, 10 * (float)Math.Sin(Math.PI * (CurrentLife / (float)Life)));
            sb.Draw(UIManager.DefaultProgressBar, new Rectangle((int)Location.X, (int)Location.Y, Width, UIManager.DefaultProgressBar.Height), null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.FlipVertically, 1);
            sb.Draw(UIManager.DefaultProgressBar, Location, new Rectangle(0, 0, (int)(Width * Percentage), UIManager.DefaultProgressBar.Height), alpha);
            if (Width == DefaultWidth)
                sb.Draw(UIManager.ProgressBarBorder, Location, alpha);
        }

        internal new void Refresh()
        {
            CurrentLife = Life;
        }
    }
}
