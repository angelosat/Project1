using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class CheckBoxNew : ButtonBase
    {
        public static Rectangle
            UnCheckedRegion = new(0, 0, 23, 23),
            CheckedRegion = new(0, 23, 23, 23);

        public static readonly Rectangle DefaultBounds = new(0, 0, 23, 23);

        Rectangle Region { get { return this.Value ? CheckedRegion : UnCheckedRegion; } }

        public override void OnPaint(SpriteBatch sb)
        {
            sb.Draw(BackgroundTexture, new Vector2(0, (Pressed && Active) ? 1 : 0), Region, UIManager.DefaultTextColor * ((MouseHover && Active) ? 1 : 0.5f));
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(25, Height / 2), new Vector2(0, 0.5f));
        }
        bool _Value;
        public bool Value
        {
            get { return this.TickedFunc != null ? this.TickedFunc() : _Value; }
            set
            {
                _Value = value;
                this.Invalidate();
            }
        }
        public void ToggleValue()
        {
            this.Value = !this.Value;
        }
        public Func<bool> TickedFunc;
        bool LastValue;
        public override void Update()
        {
            base.Update();
            if (this.TickedFunc == null)
                return;
            var nowValue = this.TickedFunc();
            if (nowValue != this.LastValue)
                this.Invalidate();
            this.LastValue = nowValue;
        }
        public CheckBoxNew() : this("")
        {
        }
        public CheckBoxNew(string text, bool check = false)
            : this(text, Vector2.Zero, check)
        { }
        public CheckBoxNew(string text, Vector2 location, bool check = false)
           : base(location)
        {
            BackgroundTexture = UIManager.TextureTickBox;
            Text = text;
            Height = 23;
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            this.Value = check;
        }
        public CheckBoxNew(string text, Action clickAction, Func<bool> tickedFunc)
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

        public CheckBoxNew SetChecked(bool condition) { Value = condition; return this; }

        protected override void OnLeftClick()
        {
            // change state only if clicked within the actual checkmark box, otherwise just select
            Rectangle bounds = this.BoundsScreen;
            base.OnLeftClick();
        }

    }
}
