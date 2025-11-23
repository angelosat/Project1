using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Slot<T> : ButtonBase, IDropTarget, ITooltippable, IContextable where T : class, ISlottable
    {
        public Func<T, string> CornerTextFunc = item => "";
        Func<DragEventArgs, DragDropEffects> _DragDropAction = (args) => DragDropEffects.None;
        public Func<DragEventArgs, DragDropEffects> DragDropAction
        { get { return _DragDropAction; } set { _DragDropAction = value; } }
        protected override void OnTextChanged()
        {

        }

        public override void OnPaint(SpriteBatch sb)
        {
            float a = (this.MouseHover && Active) ? 1 : 0.5f;
            //a *= this.Alpha.A;
            var slotcol = this.Color * a;
            sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, slotcol, 0, new Vector2(0), 1, SprFx, Depth);
            if (this.Tag is null)
            {
                sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, slotcol, 0, new Vector2(0), 1, SprFx, Depth);
                return;
            }
            var color = this.Tag.GetSlotColor();
            sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, color * a, 0, new Vector2(0), 1, SprFx, Depth);
            var rect = new Rectangle(3, 3, Width - 6, Height - 6);

            this.Tag.DrawUI(sb, new Vector2(Width, Height) * 0.5f);

            UIManager.DrawStringOutlined(sb, CornerTextFunc(Tag), new Vector2(Slot.DefaultHeight), Vector2.One, UIManager.FontBold);
        }

       
        T LastTag = default;
        string LastText = "";
        public override void Update()
        {
            base.Update();
            if (this.Tag == null)
                return;
            string newtext = this.Tag.GetCornerText();
            if (this.Tag != this.LastTag)
                this.Invalidate();
            else if (newtext != this.LastText)
                this.Invalidate();
            this.LastTag = this.Tag;
            this.LastText = newtext;
        }
        Action<ContextArgs> _ContextAction = (args) => { };
        public Action<ContextArgs> ContextAction
        {
            get { return _ContextAction; }
            set { _ContextAction = value; }
        }

        public static int DefaultHeight = 38;

        public virtual Slot<T> SetTag(T tag)
        {
            this.Tag = tag;
            return this;
        }
        T _Tag;
        public new T Tag
        {
            get { return this._Tag; }
            set
            {
                this._Tag = value;
            }
        }

        public void Clear()
        {
            Tag = null;
            IconIndex = -1;
        }

        public Slot<T> SetBottomRightText(string text)
        {
            BottomRightLabel.Text = text;
            return this;
        }

        readonly Label BottomRightLabel;
        public Slot() : this(Vector2.Zero)
        {
            Width = UIManager.SlotSprite.Width;
            Height = UIManager.SlotSprite.Height;
        }
        public Slot(Vector2 location)
            : base(location)
        {
            Blend = Color.White;
            BackgroundTexture = UIManager.SlotSprite;
            Alpha = Color.Lerp(Color.Transparent, Blend, 0.5f);
            Width = UIManager.SlotSprite.Width;
            Height = UIManager.SlotSprite.Height;
            BottomRightLabel = new Label(new Vector2(UIManager.SlotSprite.Width), "", Alignment.Horizontal.Right, Alignment.Vertical.Bottom);
            Controls.Add(BottomRightLabel);
            Tag = null;
            Text = "";
        }

        public bool AutoText = true;

        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            this.Invalidate();
        }
        public override void OnMouseLeave()
        {
            base.OnMouseLeave();
            this.Invalidate();
        }

        public override void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Color color, float opacity)
        {
            var loc = new Vector2(destRect.X, destRect.Y);
            Color c = Color.Lerp(Color.Transparent, color, opacity);
            sb.Draw(UIManager.SlotSprite, loc, sourceRect, c, 0, new Vector2(0), 1, SprFx, Depth);
            if (Tag is null)
                return;

            Tag.GetIcon().Draw(sb, loc + new Vector2(3), sourceRect);
        }

        public int IconIndex = -1;

        public event EventHandler<DragEventArgs> DragDrop;
        protected void OnDragDrop(DragEventArgs a)
        {
            DragDrop?.Invoke(this, a);
        }
        public virtual DragDropEffects Drop(DragEventArgs args)
        {
            OnDragDrop(args);
            return DragDropAction(args);
        }

        public override void GetTooltipInfo(Control tooltip)
        {
            if (CustomTooltip)
                base.GetTooltipInfo(tooltip);
            else
                Tag.GetTooltipInfo(tooltip);
        }

        public void GetContextActions(GameObject playerEntity, ContextArgs a)
        {
        }
    }
}
