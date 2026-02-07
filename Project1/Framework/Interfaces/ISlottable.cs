using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.UI;

namespace Project1.Core.Interfaces
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
