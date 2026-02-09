using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core
{
    public interface IBlockEntityComp
    {
        void Draw(Camera camera, MapBase map, IntVec3 global);
       
        void Load(SaveTag tag);
        SaveTag Save(string name);
    }
}
