using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Start_a_Town_.UI
{
    public class SlotCustom<T> : ButtonBase, IDropTarget, ITooltippable, IContextable where T : class, ITooltippable
    {
        public Func<GameObject, bool> DragDropCondition = o => true;
        public Func<T, string> CornerTextFunc = (slot) => "";
        public Action<SpriteBatch, T> PaintAction = (s, t) => { };
        Func<DragEventArgs, DragDropEffects> _dragDropAction = (args) => DragDropEffects.None;
        public Func<DragEventArgs, DragDropEffects> DragDropAction
        { 
            get => this._dragDropAction; 
            set => this._dragDropAction = value; 
        }
        protected override void OnTextChanged()
        {

        }

        public override void OnPaint(SpriteBatch sb)
        {
            float a = (this.MouseHover && this.Active) ? 1 : 0.5f;

            sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, this.Color * a, 0, new Vector2(0), 1, this.SprFx, this.Depth);
            if (this.Tag == null)
            {
                sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, this.Color * a, 0, new Vector2(0), 1, this.SprFx, this.Depth);
                return;
            }
            var color = Color.White;
            sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, color * a, 0, new Vector2(0), 1, this.SprFx, this.Depth);
        }

        public override void OnAfterPaint(SpriteBatch sb)
        {
            if (this.Tag is null)
                return;
            this.PaintAction(sb, this.Tag);
            UIManager.DrawStringOutlined(sb, this.CornerTextFunc(this.Tag), new Vector2(SlotCustom<T>.DefaultHeight), Vector2.One, UIManager.FontBold);
        }

        T LastObject = default;
        public override void Update()
        {
            base.Update();
            if (this.Tag == null)
                return;
            if (!this.Tag.Equals(this.LastObject))
                this.Invalidate();

            this.LastObject = this.Tag;
        }
        Action<ContextArgs> _ContextAction = (args) => { };
        public Action<ContextArgs> ContextAction
        {
            get => this._ContextAction;
            set => this._ContextAction = value;
        }

        public static int DefaultHeight = 38;

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }

        T _Tag;
        public new T Tag
        {
            get => this._Tag;
            set => this._Tag = value;
        }
        public void Clear()
        {
            this.Tag = default;
            this.IconIndex = -1;
        }

        public SlotCustom<T> SetBottomRightText(string text)
        {
            this.BottomRightLabel.Text = text;
            return this;
        }

        readonly Label BottomRightLabel;
        public SlotCustom()
        {
            this.Blend = Color.White;
            this.BackgroundTexture = UIManager.SlotSprite;
            this.Alpha = Color.Lerp(Color.Transparent, this.Blend, 0.5f);
            this.Width = UIManager.SlotSprite.Width;
            this.Height = UIManager.SlotSprite.Height;
            this.BottomRightLabel = new Label(new Vector2(UIManager.SlotSprite.Width), "", Alignment.Horizontal.Right, Alignment.Vertical.Bottom);
            this.Controls.Add(this.BottomRightLabel);
            this.Tag = default;
            this.Text = "";
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

        public int IconIndex = -1;

        public event EventHandler<DragEventArgs> DragDrop;
        protected void OnDragDrop(DragEventArgs a)
        {
            DragDrop?.Invoke(this, a);
        }
        public virtual DragDropEffects Drop(DragEventArgs args)
        {
            this.OnDragDrop(args);
            return this.DragDropAction(args);
        }

        public override void GetTooltipInfo(Control tooltip)
        {
            if (this.CustomTooltip)
                base.GetTooltipInfo(tooltip);
            else
                this.Tag.GetTooltipInfo(tooltip);
        }

        public void GetContextActions(GameObject playerEntity, ContextArgs a)
        {
        }
    }
}
