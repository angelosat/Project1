using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class SlotWithText : ButtonBase
    {
        public Slot Slot;
        public Label SlotText;

        static public Texture2D DefaultSprite;
        public override void Initialize()
        {
            DefaultSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-sample-narrow");
        }

        public SlotWithText(Vector2 location)
            : base(location)
        {
            this.Size = new Rectangle(0, 0, 50, Slot.DefaultHeight);
            AutoSize = true;
            Slot = new Slot();
            SlotText = new Label();
            Tag = GameObjectSlot.Empty;
        }

        public virtual SlotWithText SetSlotText(string text = "")
        {
            SlotText = new Label(Slot.BottomRight, text);
            SlotText.Location -= new Vector2(SlotText.Width, SlotText.Height);
            SlotText.MouseThrough = true;
            Controls.Add(SlotText);
            return this;
        }

        public override object Tag
        {
            get { return base.Tag; }
            set
            {
                base.Tag = value;
                Slot.Tag = value as GameObjectSlot;
                SetText(Slot.Tag.HasValue ? Slot.Tag.Object.Name : "");
            }
        }

        public ButtonBase SetTag(GameObjectSlot tag)
        {
            this.Tag = tag;
            return this;
        }

        public override ButtonBase SetText(string text)
        {
            base.SetText(text);
            Label Name = new Label(Slot.TopRight, Text);
            Controls.Clear();
            Controls.Add(Slot, SlotText ?? "<empty>".ToLabel(), Name);
            Name.MouseThrough = true;
            return this;
        }

        public override void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Color color, float opacity)
        {
            if (Controller.Instance.Mouseover != null)
                if (Controller.Instance.Mouseover.Object == this)
                DrawHighlight(sb);
            Slot.DrawSprite(sb, destRect, sourceRect, color*(Slot.MouseHover ? 1 : 0.5f), 1);
        }
    }
}
