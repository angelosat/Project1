using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class ButtonNew : ButtonBase
    {
        public float HAlign = .5f;
      
        public override int Height
        {
            get
            {
                return this.BackgroundStyle.Left.Height;
            }
            set
            {
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
                    base.Width = Math.Max((int)this.Font.MeasureString(Text).X + this.BackgroundStyle.Left.Width + this.BackgroundStyle.Right.Width + 2, value);
                else
                    base.Width = Math.Max(base.Width, value);
            }
        }
        //public override Vector2 ScreenLocation => base.ScreenLocation + Vector2.UnitY * (this.IsPushed ? 1 : 0);
        //public override Vector2 ClientLocation { get => base.ClientLocation + Vector2.UnitY * (this.IsPushed ? 1 : 0); set => base.ClientLocation = value; }

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
            set { 
                _TexBackgroundColor = value; 
                this.Invalidate(); 
            }
        }
        public override void Update()
        {
            if (this.TexBackgroundColorFunc != null)
            {
                var newCol = this.TexBackgroundColorFunc();
                if (newCol != this._TexBackgroundColor)
                {
                    this._TexBackgroundColor = newCol;
                    this.Invalidate();
                }
            }
            base.Update();
        }

        protected override void OnTextChanged()
        {
            base.OnTextChanged();
        }

        public ButtonNew() : base() {
            TexBackgroundColorFunc = DefaultBackgroundColorFunc;
            this.BackgroundStyle = UI.BackgroundStyle.LargeButton;
            this.Height = this.BackgroundStyle.Left.Height;
            Text = "";
            this.AutoSize = true;
            this.Color = Color.White * .5f;
        }

        public Color DefaultBackgroundColorFunc()
        {
            if (this.IsPushed)
                return this.Color;
            return (this.MouseHover && this.Active) ? this.Color : IdleColor;
        }
        public ButtonNew(int width) : this("", width) 
        {
            this.AutoSize = false;
            this.Width = width;
        }
       
        public ButtonNew(string text, int width = 0)
            : this()
        {
            Text = text;
            this.Width = Math.Max(this.Width, width);
        }
        public ButtonNew(Vector2 location) : this() { this.Location = location; Text = ""; Height = UIManager.DefaultButtonHeight; }
        public override void Initialize()
        {
            base.Initialize();
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
        }
        public override void OnPaint(SpriteBatch sb)
        {
            ButtonNew.Draw(sb, this, Vector2.Zero, 1);
        }
      
        static public void Draw(SpriteBatch sb, ButtonNew button, Vector2 location, float opacity = 1)
        {
            var c = Color.Lerp(Color.Transparent, button.TexBackgroundColor, opacity);
            button.BackgroundStyle.Draw(sb, button.Size, c, button.SprFx);// SpriteEffects);
            var halign = button.HAlign;
            var actualwidth = button.Width - Button.SpriteLeft.Width - Button.SpriteRight.Width;
            var pos = new Vector2(Button.SpriteLeft.Width + actualwidth * halign, Button.SpriteCenter.Height / 2 + (button.IsPushed ? 1 : 0));
            var origin = new Vector2(halign, .5f);
            UIManager.DrawStringOutlined(sb, button.Text, pos, origin, button.TextColor, button.TextOutline, button.Font);
        }
        
        public override void DrawText(SpriteBatch sb, Vector2 position, Rectangle? sourceRect, Color color, float opacity)
        {
            UIManager.DrawStringOutlined(sb, this.Text, position, this.Anchor);
        }

        static public ButtonNew CreateBig(Action leftClickAction, int width, Texture2D graphic, Func<string> textTop, Func<string> textBottom = null)
        {
            var btn = new ButtonNew(width)
            {
                AutoSize = false,
                BackgroundStyle = BackgroundStyle.LargeButton,
                LeftClickAction = leftClickAction
            };
            btn.Width = width;
            var padding = btn.BackgroundStyle.Left.Width;
            var picbox = new PictureBox(graphic) { MouseThrough = true, Location = new Vector2(padding, btn.Height / 2), Anchor = new Vector2(0, .5f) };//) { DrawAction = () => block.PaintIcon(Block.Width, Block.Height, variant.Data) });
            var label = new Label() { TextFunc = textTop, Location = picbox.TopRight + Vector2.UnitX * padding, MouseThrough = true };

            btn.AddControls(picbox, label);
            if(textBottom != null)
                btn.AddControls( new Label() { TextFunc = textBottom, Location = picbox.BottomRight + Vector2.UnitX * padding, Anchor = Vector2.UnitY, MouseThrough = true });

            return btn;
        }
   
        static public ButtonNew CreateBig(Action leftClickAction, int width, Func<string> textTop, Func<string> textBottom = null)
        {
            var btn = new ButtonNew(width)
            {
                AutoSize = false,
                BackgroundStyle = BackgroundStyle.LargeButton,
                LeftClickAction = leftClickAction,
                Padding = BackgroundStyle.LargeButton.Left.Width
            };
            btn.Width = width;
            var padding = btn.BackgroundStyle.Left.Width;
            var label = new Label() { TextFunc = textTop, Location = Vector2.One * padding, MouseThrough = true };

            //btn.AddControls(label);
            //if (textBottom != null)
            //    btn.AddControls(new Label() { TextFunc = textBottom, Location = new Vector2(padding, btn.Height - padding), Anchor = Vector2.UnitY, MouseThrough = true });
            var box = new GroupBox(btn.Width, btn.Height) { MouseThrough = true, LocationFunc = () => btn.IsPushed ? Vector2.UnitY : Vector2.Zero };
            box.AddControls(label);
            if (textBottom != null)
                box.AddControls(new Label() { TextFunc = textBottom, Location = new Vector2(padding, btn.Height - padding), Anchor = Vector2.UnitY, MouseThrough = true });
            btn.AddControls(box);
            return btn;
        }
        public static ButtonNew CreateMedium(Icon icon, Action leftClickAction)
        {
            var style = BackgroundStyle.ButtonMedium;
            var width = style.Left.Height;
            var btn = new ButtonNew(width)
            {
                AutoSize = false,
                BackgroundStyle = style,
                LeftClickAction = leftClickAction
            };
            btn.Width = width;
            var iconctrl = new PictureBox(icon.AtlasToken) { MouseThrough = true, LocationFunc = () => btn.IsPushed ? Vector2.UnitY : Vector2.Zero };
            iconctrl.Location = new(btn.Width / 2 - iconctrl.Width / 2, btn.Height / 2 - iconctrl.Height / 2);
            //var box = new GroupBox(btn.Width, btn.Height) { MouseThrough = true, LocationFunc = () => btn.IsPushed ? Vector2.UnitY : Vector2.Zero };
            //box.AddControls(iconctrl);
            //btn.AddControls(box);
            btn.AddControls(iconctrl);
            return btn;
        }
        public static ButtonNew CreateMedium(string text, Action leftClickAction)
        {
            var btn = CreateSquare(leftClickAction);
            var iconctrl = new Label(text) { MouseThrough = true, LocationFunc = () => btn.IsPushed ? Vector2.UnitY : Vector2.Zero };
            iconctrl.Location = new(btn.Width / 2 - iconctrl.Width / 2, btn.Height / 2 - iconctrl.Height / 2);
            btn.AddControls(iconctrl);
            return btn;
        }
        static ButtonNew CreateSquare(Action leftClickAction)
        {
            var style = BackgroundStyle.ButtonMedium;
            var width = style.Left.Height;
            var btn = new ButtonNew(width)
            {
                AutoSize = false,
                BackgroundStyle = style,
                LeftClickAction = leftClickAction,
            };
            return btn;
        }
    }
}
