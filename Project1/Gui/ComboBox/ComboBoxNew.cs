using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Start_a_Town_.UI
{
    [Obsolete]
    class ComboBoxNew<T> : Button where T : INamed
    {
        public enum OpenOrientation { Below, Above }
        public OpenOrientation Orientation = OpenOrientation.Below;
        public bool Open;
        readonly Panel Panel;
        public ComboBoxNew(SelectableItemList<T> items, string defaultText, int width)
        {
            this.AutoSize = false;
            this.Panel = items.ToPanel();
            items.SelectAction = (i) => { this.Text = i.Name; this.ToggleOpen(); };
            this.Text = defaultText;
            this.Width = width;
            this.Height = UIManager.DefaultButtonHeight;
        }

        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.Panel.HitTest() && this.Open)
                this.ToggleOpen();
            else
                base.HandleLButtonDown(e);
        }
        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.ToggleOpen();
        }
        protected override void OnHidden()
        {
            this.Panel.Hide();
            base.OnHidden();
        }

        private void ToggleOpen()
        {
            switch (this.Orientation)
            {
                case OpenOrientation.Below:
                    this.Panel.LocationFunc = () => this.ScreenLocation + new Vector2(0, this.Height);
                    break;

                case OpenOrientation.Above:
                    this.Panel.LocationFunc = () => this.ScreenLocation - new Vector2(0, this.Panel.Height);
                    break;
            }
            if (!this.Open)
            {
                this.Panel.Layer = this.Layer;
                this.Panel.Layer = this.Layer;
                this.Panel.Show();
            }
            else
                this.Panel.Hide();
            this.Panel.BringToFront();
            this.Open = !this.Open;
            this.Invalidate();
        }
        public override void OnPaint(SpriteBatch sb)
        {
            float a = (this.MouseHover && this.Active) ? 1 : (this.Active ? 0.5f : 0.1f);

            SpriteEffects spr = this.Open ? SpriteEffects.FlipVertically : SpriteEffects.None;
            sb.Draw(UIManager.DefaultDropdownSprite, Vector2.Zero, left, this.Color * a, 0, new Vector2(0), 1, spr, this.Depth);
            sb.Draw(UIManager.DefaultDropdownSprite, new Rectangle(left.Width, 0, this.Width - (left.Width + right.Width), 23), center, this.Color * a, 0, new Vector2(0), spr, this.Depth);
            sb.Draw(UIManager.DefaultDropdownSprite, new Vector2(this.Width - right.Width, 0), right, this.Color * a, 0, new Vector2(0), 1, spr, this.Depth);
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(this.Width / 2 - 8, this.Height / 2 + (this.Open ? 1 : 0)), new Vector2(0.5f, .5f));
        }
        static readonly Rectangle
            left = new(0, 0, 4, 23),
            center = new(4, 0, 1, 23),
            right = new(5, 0, 22, 23);

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
}
