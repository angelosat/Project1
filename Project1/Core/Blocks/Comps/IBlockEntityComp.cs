using Project1.Core.Base;
using Project1.Core.Rendering;
using Project1.Core.Simulation;

namespace Project1.Core
{
    public interface IBlockEntityComp
    {
        //void Tick(MapBase map, IBlockEntityCompContainer entity);
        void Draw(Camera camera, MapBase map, IntVec3 global);
       
        void Load(SaveTag tag);
        SaveTag Save(string name);
    }
}
