using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class DragDropSlot : DragEventArgs
    {
        public GameObject Parent;
        new public GameObjectSlot Source;
        public GameObjectSlot Slot;
        RenderTarget2D Texture;
        public TargetArgs SourceTarget, DraggedTarget;

        public DragDropSlot(GameObject parent, TargetArgs source, TargetArgs dragged, DragDropEffects effects)
        {
            this.Parent = parent;
            this.SourceTarget = source;
            this.DraggedTarget = dragged;
            this.Effects = effects;
            var rect = dragged.Slot.Object.Body.GetMinimumRectangle();
            this.Texture = new RenderTarget2D(Game1.Instance.GraphicsDevice, rect.Width, rect.Height);
            dragged.Slot.Object.Body.RenderNewererest(dragged.Slot.Object, this.Texture);
        }
        public DragDropSlot(GameObject parent, GameObjectSlot source, GameObjectSlot slot, DragDropEffects effects)
        {
            this.Parent = parent;
            this.Source = source;
            this.Slot = slot;
            this.Effects = effects;
        }

        public override void Draw(SpriteBatch sb)
        {
            var screenLocation = Controller.Instance.MouseLocation / UIManager.Scale;
            if (this.Texture is not null)
                sb.Draw(this.Texture, screenLocation, Color.White);
            UIManager.DrawStringOutlined(sb, this.DraggedTarget.Slot.Object.StackSize.ToString(), screenLocation + new Vector2(UI.Slot.DefaultHeight), Vector2.One, UIManager.FontBold);
        }
    }
}
