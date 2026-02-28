using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Project1.Framework.UI
{
    public class CheckBoxFinalNew : ButtonBaseNew
    {
        public static Rectangle
            UnCheckedRegion = new(0, 0, 23, 23),
            CheckedRegion = new(0, 23, 23, 23);

        public static readonly Rectangle DefaultBounds = new(0, 0, 23, 23);

        Rectangle Region { get { return this.TickedFunc() ? CheckedRegion : UnCheckedRegion; } }

        public override void OnPaint(SpriteBatch sb)
        {
            sb.Draw(BackgroundTexture, new Vector2(0, (Pressed && Active) ? 1 : 0), Region, UIManager.DefaultTextColor * ((MouseHover && Active) ? 1 : 0.5f));
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(25, Height / 2), new Vector2(0, 0.5f));
        }
        //public override void Update()
        //{
        //    base.Update();
        //}
        readonly Func<bool> TickedFunc;

        public CheckBoxFinalNew(Action clickAction, Func<bool> tickedFunc) : this("", clickAction, tickedFunc)
        {

        }
        public CheckBoxFinalNew(string text, Action clickAction, Func<bool> tickedFunc)
        {
            BackgroundTexture = UIManager.TextureTickBox;
            Text = text;
            Height = 23;
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            this.LeftClickAction = clickAction;
            this.TickedFunc = tickedFunc;
        }

        protected override void OnTextChanged()
        {
            base.OnTextChanged();
            if (!string.IsNullOrWhiteSpace(this.Text))
                this.Width += BackgroundTexture.Width + 5;
        }

        protected override void OnLeftClick()
        {
            // change state only if clicked within the actual checkmark box, otherwise just select
            Rectangle bounds = this.BoundsScreen;
            base.OnLeftClick();
        }

    }
    public class CheckBoxFinal : ButtonBase
    {
        public static Rectangle
            UnCheckedRegion = new(0, 0, 23, 23),
            CheckedRegion = new(0, 23, 23, 23);

        public static readonly Rectangle DefaultBounds = new(0, 0, 23, 23);

        Rectangle Region { get { return this.TickedFunc() ? CheckedRegion : UnCheckedRegion; } }

        public override void OnPaint(SpriteBatch sb)
        {
            sb.Draw(BackgroundTexture, new Vector2(0, (Pressed && Active) ? 1 : 0), Region, UIManager.DefaultTextColor * ((MouseHover && Active) ? 1 : 0.5f));
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(25, Height / 2), new Vector2(0, 0.5f));
        }
        public override void Update()
        {
            base.Update();
        }
        readonly Func<bool> TickedFunc;

        public CheckBoxFinal(Action clickAction, Func<bool> tickedFunc) : this("", clickAction, tickedFunc)
        {
            
        }
        public CheckBoxFinal(string text, Action clickAction, Func<bool> tickedFunc)
        {
            BackgroundTexture = UIManager.TextureTickBox;
            Text = text;
            Height = 23;
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            this.LeftClickAction = clickAction;
            this.TickedFunc = tickedFunc;
        }

        protected override void OnTextChanged()
        {
            base.OnTextChanged();
            if (!string.IsNullOrWhiteSpace(this.Text))
                this.Width += BackgroundTexture.Width + 5;
        }

        protected override void OnLeftClick()
        {
            // change state only if clicked within the actual checkmark box, otherwise just select
            Rectangle bounds = this.BoundsScreen;
            base.OnLeftClick();
        }

    }
}
