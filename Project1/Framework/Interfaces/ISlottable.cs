using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Project1.Framework.Interfaces
{
    
    public interface ISlottable : ITooltippable
    {
        //string GetName();
        string Name { get; }
        Icon GetIcon();
        Color GetSlotColor();
        string GetCornerText();
        void DrawUI(SpriteBatch sb, Vector2 pos);
    }
}
