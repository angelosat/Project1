using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class FloatingText : Label
    {
        static public void Create(GameObject entity, string text, Action<FloatingText> initializer = null)
        {
            Create(entity.Map, () => entity.Global + entity.Physics.Height * Vector3.UnitZ, text, initializer);
        }
        static public void Create(MapBase map, Func<Vector3> global, string text, Action<FloatingText> initializer = null)
        {
            if (map is null || map != Ingame.Net.Map)
                return;
            var ft = new FloatingText(global, text);
            ft.Font = UIManager.FontBold;
            initializer?.Invoke(ft);
            ft.Show();
        }
        static public void Create(MapBase map, Vector3 global, string text, Action<FloatingText> initializer) => Create(map, () => global, text, initializer);

        static readonly Random Random = new Random();
        const float Gravity = 0.05f;// 0.1f;

        public event EventHandler<EventArgs> Finished;
        void OnFinished()
        {
            Finished?.Invoke(this, EventArgs.Empty);
        }

        static public float Speed = 0.5f, Life = 3 * Ticks.PerSecond;
        float ScaleTime;
        float ScaleLength;
        float CurrentLife;
        Func<Vector3> GetGlobal;
        Vector2 Velocity;
        Vector2 Position;

        public FloatingText(GameObject obj, string text)
            : base(Vector2.Zero, text)
        {
            this.Layer = UIManager.LayerSpeechbubbles;
            Text = text;
            CurrentLife = Life;
            ScaleTime = ScaleLength = 20;
            this.GetGlobal = () => obj.Global;
            float angle = (float)((Math.PI / 3f) * (1 + Random.NextDouble()));
            this.Velocity = new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle));
            this.Velocity *= 3;
            this.Position = Vector2.Zero;
        }
        public FloatingText(Func<Vector3> global, string text)
            : base(Vector2.Zero, text)
        {
            this.Layer = UIManager.LayerSpeechbubbles;
            Text = text;
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
            sb.Draw(this.Texture, this.Location, null, this.Color * Opacity, Rotation, this.Texture.Bounds.Center.ToVector(), scale, SpriteEffects.None, 0);
        }
    }
}
