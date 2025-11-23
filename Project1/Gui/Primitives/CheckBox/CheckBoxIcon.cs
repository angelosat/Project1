using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class CheckBoxIcon : ButtonBase
    {
        static Rectangle
            UnCheckedRegion = new Rectangle(0, 0, 23, 23),
            CheckedRegion = new Rectangle(0, 23, 23, 23);

        Rectangle Region { get { return this.Value ? CheckedRegion : UnCheckedRegion; } }

        public Func<bool> ValueFunc;
        bool _Value, LastValue;
        public CheckBoxIcon(Func<bool> valueFunc, Action tickAction)
        {
            this.BackgroundTexture = UIManager.TextureTickBox;
            this.Width = CheckedRegion.Width;
            this.Height = CheckedRegion.Height;
            this.ValueFunc = valueFunc;
            this.LeftClickAction = tickAction;
        }
        public override void OnPaint(SpriteBatch sb)
        {
            sb.Draw(BackgroundTexture, new Vector2(0, (Pressed && Active) ? 1 : 0), Region, UIManager.DefaultTextColor * ((MouseHover && Active) ? 1 : 0.5f));
        }
        public bool Value
        {
            get { return this.ValueFunc?.Invoke() ?? this._Value; }
            set
            {
                _Value = value;
                this.Invalidate();
            }
        }
        
        public override void Update()
        {
            base.Update();
            if (this.ValueFunc == null)
                return;
            var nowValue = this.ValueFunc();
            if (nowValue != this.LastValue)
                this.Invalidate();
            this.LastValue = nowValue;
        }
    }
}
