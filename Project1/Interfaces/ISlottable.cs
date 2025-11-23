using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    
    public interface ISlottable : ITooltippable
    {
        string GetName();
        Icon GetIcon();
        Color GetSlotColor();
        string GetCornerText();
        void DrawUI(SpriteBatch sb, Vector2 pos);
    }
}
