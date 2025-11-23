using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public class InteractionBar : Label
    {
        GameObject _Object;
        public GameObject Object
        {
            get { return _Object; }
            set
            {
                _Object = value;
                PositionComponent = _Object.Transform; 
            }
        }
        string _DisplayName;
        public string DisplayName
        {
            get { return _DisplayName; }
            set
            {
                _DisplayName = value;
                if (value is not null)
                    if (value.Length > 0)
                        Text = (string)Object.GetType().GetProperty(value).GetValue(Object, null);
            }
        }
        string _DisplayPercentage;
        public string DisplayPercentage
        {
            get
            {
                return _DisplayPercentage;
            }
            set
            {
                _DisplayPercentage = value;
                if (value != null)
                    if (value.Length > 0)
                        Percentage = (float)Object.GetType().GetProperty(value).GetValue(Object, null);
            }
        }
        protected double _t = 0;
        public double Percentage
        {
            get
            {
                return _t;
            }
            set
            {
                _t = Math.Min(Math.Max(value, 0), 1);
            }
        }

        public new static int DefaultHeight = UIManager.DefaultProgressBar.Height;
        public static int DefaultWidth = UIManager.DefaultProgressBar.Width;
        PositionComponent PositionComponent;
        public override void Draw(SpriteBatch sb)
        {
            Vector2 screenLoc;
            Rectangle bounds;
            if (PositionComponent != null)
            {
                Vector3 global = PositionComponent.Global;
                bounds = (ScreenManager.CurrentScreen as Ingame).Camera.GetScreenBounds((int)global.X, (int)global.Y, (int)global.Z, new Rectangle(0, 0, 1, 1));
                screenLoc = new Vector2(bounds.X + bounds.Width/2 - Width / 2, bounds.Top - Height);
            }
            else
                screenLoc = ScreenLocation;
            
            sb.Draw(UIManager.DefaultProgressBar, new Rectangle((int)screenLoc.X, (int)screenLoc.Y, Width, UIManager.DefaultProgressBar.Height), null, Color.Lerp(Color.Orange, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.FlipVertically, 1);
            sb.Draw(UIManager.DefaultProgressBar, screenLoc, new Rectangle(0, 0, (int)(Width * Percentage), UIManager.DefaultProgressBar.Height), Color.Orange);
            if (Width == DefaultWidth)
                sb.Draw(UIManager.ProgressBarBorder, screenLoc, Color.White);
        }

        static public void Draw(SpriteBatch sb, Vector2 screenLoc, int width, float percentage)
        { 
            var rect = new Rectangle((int)screenLoc.X, (int)screenLoc.Y, width, UIManager.DefaultProgressBar.Height);
            sb.Draw(UIManager.DefaultProgressBar, rect, null, Color.Lerp(Color.Orange, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
            sb.Draw(UIManager.DefaultProgressBar, screenLoc, new Rectangle(0, 0, (int)(width * percentage), UIManager.DefaultProgressBar.Height), Color.Orange);
            if (width == DefaultWidth)
                sb.Draw(UIManager.ProgressBarBorder, screenLoc, Color.White);
        }
        static public void Draw(SpriteBatch sb, Vector2 screenLoc, int width, float percentage, float scale)
        {
            var w = (int)(width * scale);
            var h = (int)(UIManager.DefaultProgressBar.Height * scale);
            var x = (int)screenLoc.X - w/2;
            var y = (int)screenLoc.Y - h/2;
            var rect = new Rectangle(x, y, w, h);
            sb.Draw(UIManager.DefaultProgressBar, rect, null, Color.Lerp(Color.Orange, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);//1);//0.1f);
            sb.Draw(UIManager.DefaultProgressBar, new Vector2(x, y), new Rectangle(0, 0, (int)(w * percentage), h), Color.Orange);
            if (width == DefaultWidth)
                sb.Draw(UIManager.ProgressBarBorder, new Vector2(x, y), null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0f);//0.05f);
        }
    }
}
