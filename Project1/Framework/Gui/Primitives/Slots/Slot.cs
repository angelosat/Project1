using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Slot : ButtonBase, IDropTarget, ITooltippable, IContextable
    {
        GameObject LastObject = null;
        int LastStackSize = 0;
        public Func<GameObject, bool> DragDropCondition = o => true;
        public Func<GameObjectSlot, string> CornerTextFunc = (slot) => slot.StackSize > 1 ? slot.StackSize.ToString() : "";
        Func<DragEventArgs, DragDropEffects> _DragDropAction = (args) => DragDropEffects.None;
        public Func<DragEventArgs, DragDropEffects> DragDropAction 
        { get { return _DragDropAction; } set { _DragDropAction = value; } }
        protected override void OnTextChanged()
        {

        }
        public Action DragDropCreateAction = () => { };

        public override void OnPaint(SpriteBatch sb)
        {
            float a = (this.MouseHover && Active) ? 1 : 0.5f;
            var slotcol = this.Color * a;
            sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, slotcol, 0, new Vector2(0), 1, SprFx, Depth);
            if (!this.Tag.HasValue)
            {
                sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, slotcol, 0, new Vector2(0), 1, SprFx, Depth);
                return;
            }
            var color = this.Tag.Object.GetInfo().GetQualityColor();
            sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, color * a, 0, new Vector2(0), 1, SprFx, Depth);
        }
     
        public override void OnAfterPaint(SpriteBatch sb)
        {
            if (!this.Tag.HasValue)
                return;
            GraphicsDevice gd = sb.GraphicsDevice;
            var sprite = this.Tag.Object.Body.Sprite;
            Rectangle rect = new Rectangle(3, 3, Width - 6, Height - 6);
            var loc = new Vector2(rect.X, rect.Y);
            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            MySpriteBatch mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            fx.Parameters["Viewport"].SetValue(new Vector2(this.Size.Width, this.Size.Height));
            fx.Parameters["s"].SetValue(Sprite.Atlas.Texture);
            fx.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            var body = this.Tag.Object.Body;
            var scale = 1;
            loc += sprite.OriginGround;
            body.DrawGhost(this.Tag.Object, mysb, loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
            mysb.Flush();
            UIManager.DrawStringOutlined(sb, CornerTextFunc(Tag), new Vector2(Slot.DefaultHeight), Vector2.One, UIManager.FontBold);
        }
       
        public override void Update()
        {
            base.Update();
            if (this.Tag is null)
                return;
            if (this.Tag.Object != this.LastObject)
                this.Invalidate();
            else if (this.Tag.StackSize != this.LastStackSize)
                this.Invalidate();
            this.LastObject = this.Tag.Object;
            this.LastStackSize = this.Tag.StackSize;
        }
        Action<ContextArgs> _ContextAction = (args) => { };
        public Action<ContextArgs> ContextAction
        {
            get { return _ContextAction; }
            set { _ContextAction = value; }
        }
       
        public static int DefaultHeight = 38;

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }

        public virtual Slot SetTag(GameObjectSlot tag)
        {
            this.Tag = tag;
            return this;
        }
        GameObjectSlot _Tag;
        public new GameObjectSlot Tag
        {
            get { return this._Tag; }
            set
            {
                this._Tag = value;
            }
        }

        public virtual void Clear()
        {
            Tag = null;
            IconIndex = -1;
        }

        public Slot SetBottomRightText(string text)
        {
            BottomRightLabel.Text = text;
            return this;
        }

        Label BottomRightLabel;
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
            Tag = GameObjectSlot.Empty;
            Text = "";
        }
        public Slot(GameObjectSlot objSlot)
            : this()
        {
            this.Tag = objSlot;
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
            Vector2 loc = new Vector2(destRect.X, destRect.Y);
            Color c = Color.Lerp(Color.Transparent, color, opacity);
            sb.Draw(UIManager.SlotSprite, loc, sourceRect, c, 0, new Vector2(0), 1, SprFx, Depth);
        }


        static public void Draw(SpriteBatch sb, GameObjectSlot slot, Vector2 loc, SpriteEffects sprFx, float opacity, string text = "")
        {
            Color color = slot.Object.GetInfo().GetQualityColor();

            sb.Draw(UIManager.SlotSprite, loc, null, color * opacity, 0, new Vector2(0), 1, sprFx, 0);

            if (slot.Object == null)
                return;

            if (text != "")
                UIManager.DrawStringOutlined(sb, text, loc + new Vector2(Slot.DefaultHeight), Vector2.One);
            else
                if (slot.StackSize > 1)
                    UIManager.DrawStringOutlined(sb, slot.StackSize.ToString(), loc + new Vector2(Slot.DefaultHeight), Vector2.One);
        }

        public int IconIndex = -1;

        public event EventHandler<DragEventArgs> DragDrop;
        protected virtual void OnDragDrop(DragEventArgs a)
        {
            if (DragDrop != null)
                DragDrop(this, a);
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

        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.OnMouseLeftPress(e);
            if (this.Pressed)
                DragDropManager.Start(this, this.DragDropCreateAction);
        }
    }
}
