using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Bar : ButtonBase
    {
        public bool Invert;
        public override Texture2D BackgroundTexture
        {
            get => UIManager.DefaultProgressBar;
            set { }
        }

        public IProgressBar Object { get; set; }

        public float Percentage => (Object != null) ? this.Object.Percentage : 0;
        
        float LastPercentage = 0;
        string LastText = "";

        public override void OnPaint(SpriteBatch sb)
        {
            var percentage = Invert ? (1 - this.Percentage) : this.Percentage;
            var fill = (int)Math.Round(this.Width * percentage);
            sb.Draw(this.BackgroundTexture, Vector2.Zero, new Rectangle(0, 0, fill, this.Height), Color);//Color.White);
            var txt = (this.TextFunc != null ? this.TextFunc() : "");
            UIManager.DrawStringOutlined(sb, Name + txt, Dimensions * 0.5f, new Vector2(0.5f));
        }

        public Bar()
        {
            this.Height = UIManager.DefaultProgressBarStrip.Bounds.Height;
            this.Width = 100;
            this.BackgroundColor = Color.Black * 0.5f;
        }
        public Bar(IProgressBar progress)
            : this()
        {
            this.Object = progress;
        }
        public Bar(IProgressBar progress, int width, Func<string> textFunc)
            : this(progress)
        {
            this.Width = width;
            this.TextFunc = textFunc;
        }
        public override void Update()
        {
            if (Math.Round(this.LastPercentage * this.Width) != Math.Round(Percentage * this.Width))
                this.Invalidate();
            
            var nextText = this.TextFunc?.Invoke() ?? this.Text;
            if(nextText != this.LastText)
            {
                this.Text = nextText;
                this.Invalidate();
            }
            this.LastText = this.Text;
            base.Update();
            this.LastPercentage = this.Percentage;
        }

        static public void Draw(SpriteBatch sb, Camera camera, GameObject parent, string text, float percentage)
        {
            var bounds = camera.GetScreenBounds(parent.Global, parent.GetSprite().GetBounds());
            var scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            var barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            var textLoc = new Vector2(barLoc.X, scrLoc.Y);
            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, percentage);
            UIManager.DrawStringOutlined(sb, text, textLoc, Alignment.Horizontal.Left, Alignment.Vertical.Center, 0.5f);
        }
        static public void Draw(SpriteBatch sb, Camera camera, Vector3 global, string text, float percentage, float scale)
        {
            text = camera.Zoom > 2 ? text : "";
            var scrLoc = camera.GetScreenPositionFloat(global);
            InteractionBar.Draw(sb, scrLoc, InteractionBar.DefaultWidth, percentage, scale);
            UIManager.DrawStringOutlined(sb, text, scrLoc, Color.Black, Color.White, 1, Alignment.Horizontal.Center, Alignment.Vertical.Center, 0.5f);
        }
    }

    public class BarNew : ButtonBase
    {
        public bool Invert;
        public override Texture2D BackgroundTexture
        {
            get => UIManager.DefaultProgressBar;
            set { }
        }

        public float Percentage => Value / Max;

        float LastPercentage = 0;
        string LastText = "";
        Func<float> MaxFunc, ValueFunc;
        internal string Format { set => this.TextFunc = () => string.Format(value, this.Value, this.Max); }

        float Max => MaxFunc();
        float Value => ValueFunc();

        public override void OnPaint(SpriteBatch sb)
        {
            var percentage = Invert ? (1 - this.Percentage) : this.Percentage;
            var fill = (int)Math.Round(this.Width * percentage);
            sb.Draw(this.BackgroundTexture, Vector2.Zero, new Rectangle(0, 0, fill, this.Height), Color);//Color.White);
            var txt = this.TextFunc?.Invoke() ?? "";
            //txt = string.Format(txt, this.Value, this.Max);
            UIManager.DrawStringOutlined(sb, Name + txt, Dimensions * 0.5f, new Vector2(0.5f));
        }

        public BarNew()
        {
            this.Height = UIManager.DefaultProgressBarStrip.Bounds.Height;
            this.Width = 100;
            this.BackgroundColor = Color.Black * 0.5f;
        }
        public BarNew(Func<float> maxGetter, Func<float> valueGetter)
            : this()
        {
            this.MaxFunc = maxGetter;
            this.ValueFunc = valueGetter;
        }
        public BarNew(Func<float> maxGetter, Func<float> valueGetter, int width, Func<string> textFunc)
            : this(maxGetter, valueGetter)
        {
            this.Width = width;
            this.TextFunc = textFunc;
        }
        public override void Update()
        {
            if (Math.Round(this.LastPercentage * this.Width) != Math.Round(Percentage * this.Width))
                this.Invalidate();

            var nextText = this.TextFunc?.Invoke() ?? this.Text;
            if (nextText != this.LastText)
            {
                this.Text = nextText;
                this.Invalidate();
            }
            this.LastText = this.Text;
            base.Update();
            this.LastPercentage = this.Percentage;
        }

        static public void Draw(SpriteBatch sb, Camera camera, GameObject parent, string text, float percentage)
        {
            var bounds = camera.GetScreenBounds(parent.Global, parent.GetSprite().GetBounds());
            var scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            var barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            var textLoc = new Vector2(barLoc.X, scrLoc.Y);
            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, percentage);
            UIManager.DrawStringOutlined(sb, text, textLoc, Alignment.Horizontal.Left, Alignment.Vertical.Center, 0.5f);
        }
        static public void Draw(SpriteBatch sb, Camera camera, Vector3 global, string text, float percentage, float scale)
        {
            text = camera.Zoom > 2 ? text : "";
            var scrLoc = camera.GetScreenPositionFloat(global);
            InteractionBar.Draw(sb, scrLoc, InteractionBar.DefaultWidth, percentage, scale);
            UIManager.DrawStringOutlined(sb, text, scrLoc, Color.Black, Color.White, 1, Alignment.Horizontal.Center, Alignment.Vertical.Center, 0.5f);
        }
    }
}
