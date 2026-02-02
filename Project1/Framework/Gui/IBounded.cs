using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public interface IBounded
    {
        Rectangle Bounds { get; }
    }
    public interface IBoundedCollection
    {
        Rectangle ContainerSize { get; }
        IBounded[] Children { get; }
    }
    public class BoundedCollection
    {
        Rectangle ContainerSize { get; }
        Rectangle[] Children { get; }
    }
}
