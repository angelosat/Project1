using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Framework.Helpers;
using System;

namespace Project1.Framework.UI.Primitives
{
    public class BarFinal : ButtonBaseNew
    {
        IDisposable subscription;
        public override Texture2D BackgroundTexture => UIManager.DefaultProgressBar;
        readonly ProgressInt Progress; 
        public bool Invert;
        public BarFinal(ProgressInt progress, Func<string> textGetter = null)
        {
            this.Progress = progress;
            this.Height = UIManager.DefaultProgressBarStrip.Bounds.Height;
            this.Width = 100;
            this.BackgroundColor = Color.Black * 0.5f;
            this.subscription = progress.Subscribe(() => this.Invalidate(true));
            this.TextFunc = textGetter;
            this.HoverFunc = () => $"{this.TextFunc()} {progress}";
        }
      
        internal override void OnRemoved()
        {
            this.subscription.Dispose();
            this.subscription = null;
            base.OnRemoved();
        }
        
        public override void OnPaint(SpriteBatch sb)
        {
            var percentage = Invert ? (1 - this.Progress.Percentage) : this.Progress.Percentage;
            var fill = (int)System.Math.Round(this.Width * percentage);
            sb.Draw(this.BackgroundTexture, Vector2.Zero, new Rectangle(0, 0, fill, this.Height), Color);//Color.White);
            var txt = (this.TextFunc != null ? this.TextFunc() : "");
            UIManager.DrawStringOutlined(sb, Name + txt, Dimensions * 0.5f, new Vector2(0.5f));
        }
    }
}
