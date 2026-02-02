using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class BarSigned : ButtonBase
    {
        public bool Invert;
        public override Texture2D BackgroundTexture
        {
            get
            {
                return UIManager.DefaultProgressBar;
            }
            set
            {
            }
        }

        public IProgressBar Object { get; set; }
        public float Percentage => (Object != null) ? this.Object.Percentage : 0;
        float LastPercentage = 0;
        string LastText = "";

        public override void OnPaint(SpriteBatch sb)
        {
            var percentage = Invert ? (1 - this.Percentage) : this.Percentage;
            var middle = this.Width * .5f;
            var x = this.Object.Percentage > 0 ? middle : middle * (1 + percentage);
            var color = this.Object.Percentage > 0 ? Color.Lime : Color.Red;
            var w = (int)(this.Width * Math.Abs(percentage) / 2f);
            sb.Draw(this.BackgroundTexture, new Vector2(x, 0), new Rectangle(0, 0, w, this.Height), color);
            var txt = (this.TextFunc != null ? this.TextFunc() : "");
            UIManager.DrawStringOutlined(sb, Name + txt, Dimensions * 0.5f, new Vector2(0.5f));
        }

        public BarSigned()
        {
            this.Height = UIManager.DefaultProgressBarStrip.Bounds.Height;
            this.Width = 100;// 200;
            this.BackgroundColor = Color.Black * 0.5f;
        }
        public BarSigned(IProgressBar progress)
            : this()
        {
            this.Object = progress;
        }
        public override void Update()
        {
            if ((int)(this.LastPercentage * this.Width) != (int)(Percentage * this.Width))
                this.Invalidate();

            if(this.Text != this.LastText)
                this.Invalidate();
            this.LastText = this.Text;

            base.Update();
            this.LastPercentage = this.Percentage;
        }
    }
}
