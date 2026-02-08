using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project1.Framework.UI
{
    
    public interface ISlottable : ITooltippable
    {
        string Name { get; }
        Icon GetIcon();
        Color GetSlotColor();
        string GetCornerText();
        void DrawUI(SpriteBatch sb, Vector2 pos);
    }
}
