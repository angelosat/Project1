using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Project1.Framework.UI
{
    public class ButtonFinal : ButtonBaseNew
    {
        public float HAlign = .5f;
        public static Rectangle
            SpriteLeft = new(0, 0, 4, 23),
            SpriteCenter = new(4, 0, 1, 23),
            SpriteRight = new(5, 0, 4, 23);
        public static int DefaultHeight = 23;
        public override int Padding { get => SpriteLeft.Width; }
        public override int Height
        {
            get
            {
                return DefaultHeight;
            }
            set
            {
                base.Height = value;
            }
        }
        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                if (this.AutoSize)
                    base.Width = System.Math.Max((int)this.Font.MeasureString(Text).X + SpriteLeft.Width + SpriteRight.Width + 2, value);
                else
                    base.Width = value;
            }
        }

        public Color IdleColor = Color.White * 0.1f;
        Color _TexBackgroundColor;
        public Func<Color> TexBackgroundColorFunc;
        public virtual Color TexBackgroundColor
        {
            get
            {
                if (TexBackgroundColorFunc == null)
                    return _TexBackgroundColor;
                else
                    return TexBackgroundColorFunc();
            }
            set
            {
                _TexBackgroundColor = value;
                this.Invalidate();
            }
        }
        //public override void Update()
        //{
        //    if (this.TexBackgroundColorFunc != null)
        //    {
        //        var newCol = this.TexBackgroundColorFunc();
        //        if (newCol != this._TexBackgroundColor)
        //        {
        //            this._TexBackgroundColor = newCol;
        //            this.Invalidate();
        //        }
        //    }
        //    base.Update();
        //}

        protected override void OnTextChanged()
        {
            this.Invalidate();
            //this.Validate();
        }
        //public override void Validate(bool cascade = false)
        //{
        //    this.Text = this.TextFunc?.Invoke() ?? this.Text;
        //    base.Validate(cascade);
        //}
        
        static public int GetWidth(SpriteFont font, string txt)
        {
            var textw = (int)font.MeasureString(txt).X;
            var w = textw + SpriteLeft.Width + SpriteRight.Width + 2;
            return w;
        }
        public Color DefaultBackgroundColorFunc()
        {
            if (this.IsPushed)
                return this.Color;
            return (this.MouseHover && this.Active) ? this.Color : IdleColor;
        }
        public ButtonFinal() : base()
        {
            this.Color = Color.White * .5f;
            TexBackgroundColorFunc = DefaultBackgroundColorFunc;
            Height = UIManager.DefaultButtonHeight;
            Text = "";
            this.AutoSize = true;
        }
        public ButtonFinal(int width) : this("", width)
        {
            this.Width = width;
            if (width > 0)
                this.AutoSize = false;
        }
        public ButtonFinal(SpriteFont font, string text, int width = 0)
            : this()
        {
            this.Font = font;
            Text = text;
            this.Width = System.Math.Max(this.Width, width);
        }
        public ButtonFinal(string text, Action action, int width = 0) : this(text, width)
        {
            this.LeftClickAction = action;
        }
        public ButtonFinal(Func<string> text, Action action, int width = 0) : this(width)
        {
            this.TextFunc = text;
            this.LeftClickAction = action;
        }
        public ButtonFinal(string text, int width = 0)
            : this()
        {
            Text = text;
            if (width > 0)
                this.AutoSize = false;
            this.Width = System.Math.Max(this.Width, width);
        }
        public ButtonFinal(Vector2 location) : this() { this.Location = location; Text = ""; Height = UIManager.DefaultButtonHeight; }
        public ButtonFinal(Vector2 Location, int width, String label = "")
            : this(Location)
        {
            Text = label;
            this.Width = System.Math.Max(this.Width + SpriteLeft.Width + SpriteRight.Width + 2, width);
            this.Height = UIManager.DefaultButtonHeight;
        }

        public override void Initialize()
        {
            base.Initialize();
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);

        }

        public override void OnPaint(SpriteBatch sb)
        {
            this.Text = this.TextFunc?.Invoke() ?? "";// this.Text;
            ButtonFinal.Draw(sb, this, Vector2.Zero, 1);// (MouseHover || IsPressed) ? 1 : 0.5f);
        }

        static public void Draw(SpriteBatch sb, ButtonFinal button, Vector2 location, float opacity = 1)
        {
            DrawSprite(sb, button, new Rectangle((int)location.X, (int)location.Y, button.Width, button.Height),
                button.TexBackgroundColor,
                opacity,
                button.SprFx);
            var halign = button.HAlign;
            var actualwidth = button.Width - ButtonFinal.SpriteLeft.Width - ButtonFinal.SpriteRight.Width;

            var pos = new Vector2(ButtonFinal.SpriteLeft.Width + actualwidth * halign, ButtonFinal.SpriteCenter.Height / 2 + (button.IsPushed ? 1 : 0));

            var origin = new Vector2(halign, .5f);
            UIManager.DrawStringOutlined(sb, button.Text, pos, origin, button.TextColor, button.TextOutline, button.Font);
        }

        static public void DrawSprite(SpriteBatch sb, ButtonFinal button, Rectangle destRect, Color color, float opacity, SpriteEffects sprFx)
        {
            SpriteEffects fx = button.Active ? sprFx : SpriteEffects.FlipVertically;
            Color c = Color.Lerp(Color.Transparent, color, opacity);
            sb.Draw(UIManager.DefaultButtonSprite, new Vector2(destRect.X, destRect.Y), Button.SpriteLeft, c, 0, Vector2.Zero, 1, fx, 0);
            sb.Draw(UIManager.DefaultButtonSprite,
                new Rectangle(
                    destRect.X + ButtonFinal.SpriteLeft.Width,
                    destRect.Y,
                    destRect.Width - ButtonFinal.SpriteLeft.Width - ButtonFinal.SpriteRight.Width,
                    SpriteCenter.Height),
                ButtonFinal.SpriteCenter, c, 0, Vector2.Zero, fx, 0);
            sb.Draw(UIManager.DefaultButtonSprite, new Vector2(destRect.X + destRect.Width - ButtonFinal.SpriteRight.Width, destRect.Y), ButtonFinal.SpriteRight, c, 0, Vector2.Zero, 1, fx, 0);
        }
        public override void DrawText(SpriteBatch sb, Vector2 position, Rectangle? sourceRect, Color color, float opacity)
        {
            UIManager.DrawStringOutlined(sb, this.Text, position, this.Anchor);
        }

        internal static int GetMaxWidth(IEnumerable<string> enumerable)
        {
            int max = 0;
            foreach (var item in enumerable)
            {
                var w = GetWidth(UIManager.Font, item);
                if (w > max)
                    max = w;
            }
            return max;
        }
    }
}
