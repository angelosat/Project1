using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public interface IWindowManager
    {
        Vector2 Size { get; }
        Vector2 Mouse { get; }
        Rectangle MouseRect { get; }
        int Width { get; }
        int Height { get; }
        Vector2 Center { get; }
        Element ActiveControl { get; set; }
        Element FocusedControl { get; set; }
        void Add(Element control);
        bool Remove(Element control);
        bool Contains(Element control);
        Rectangle FindBestUncoveredRectangle(Vector2 dimensions);
        void Block(bool enabled);
    }
    //public abstract class IWindowManager
    //{
    //    public abstract Vector2 Size { get; }
    //    public abstract Vector2 Mouse { get; }
    //    public abstract Rectangle MouseRect { get; }
    //    public abstract int Width { get; }
    //    public abstract int Height { get; }
    //    public abstract Vector2 Center { get; }
    //}
}
