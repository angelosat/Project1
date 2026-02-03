using Project1.Framework.Base;

namespace Start_a_Town_
{
    public interface IItemCreationSystem
    {
        public Entity Create(Def profile, ItemCreationArgs args = null);
    }
}
