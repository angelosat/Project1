using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class FloatingTextEx : GroupBox
    {
        public class Segment
        {
            public string Text;
            public Color Color;
            public Segment(string text, Color color)
            {
                this.Text = text;
                this.Color = color;
            }
        }

        static Random Random = new Random();
        const float Gravity = 0.05f;

        public event EventHandler<EventArgs> Finished;
        void OnFinished()
        {
            if (Finished != null)
                Finished(this, EventArgs.Empty);
        }

        static public float Speed = 0.5f, Life = 3 * Ticks.PerSecond;
        float ScaleTime;
        float ScaleLength;
        float CurrentLife;
        Func<Vector3> GetGlobal;
        Vector2 Velocity;
        Vector2 Position;
        public FloatingTextEx(GameObject parent, params Segment[] segments)
        {
            this.Layer = UIManager.LayerSpeechbubbles;
            foreach (var s in segments)
            {
                var label = new Label(s.Text) { Location = this.Controls.TopRight, Font = UIManager.FontBold, TextColorFunc = () => s.Color };
                this.Controls.Add(label); 
            }
            CurrentLife = Life;
            ScaleTime = ScaleLength = 20;
            this.GetGlobal = () => parent.Global;
            float angle = (float)((Math.PI / 3f) * (1 + Random.NextDouble()));
            this.Velocity = new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle));
            this.Velocity *= 3;
            this.Position = Vector2.Zero;
        }
        public FloatingTextEx(Func<Vector3> global, params Segment[] segments)
        {
            this.Layer = UIManager.LayerSpeechbubbles;
            foreach (var s in segments)
            {
                var label = new Label(s.Text) { Location = this.Controls.TopRight, Font = UIManager.FontBold, TextColorFunc = () => s.Color };
                this.Controls.Add(label);
            }
            CurrentLife = Life;
            ScaleTime = ScaleLength = 20;
            this.GetGlobal = global;
            float angle = (float)((Math.PI / 3f) * (1 + Random.NextDouble()));
            this.Velocity = new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle));
            this.Velocity *= 3;
            this.Position = Vector2.Zero;
        }
        
        public override void Update()
        {
            Camera cam = ScreenManager.CurrentScreen.Camera;

            var global = this.GetGlobal();
            Vector2 origin = cam.GetScreenPosition(global);
            Location = origin + Position;
            Position += Velocity;
            Velocity.Y += Gravity;
            Opacity = (float)Math.Sin(Math.PI * (CurrentLife / (float)Life) / 2f);
            CurrentLife -= 1;
            if(ScaleTime>0)
                ScaleTime--;
            if (CurrentLife < 0)
            {
                OnFinished();
                Hide();
            }
            base.Update();
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            float p = (ScaleTime / ScaleLength);
            float scaleFactor = (float)Math.Sin(p * Math.PI) * 5;
            float scale = 1 + scaleFactor;
            var color = this.Color * Opacity;
            var center = this.Size.Center.ToVector();
            foreach (var s in this.Controls)
            {
                var origin = center - s.Location;
                sb.Draw(s.Texture, this.Location + s.Location + origin, null, color, Rotation, origin, scale, SpriteEffects.None, 0);
            }
        }
    }
}
