using Project1.Core.Rendering;
using Project1.Framework;

namespace Project1.Core.Blocks.Comps;

public interface IBlockEntityComp
{
    void Draw(Camera camera, Renderer renderer, IntVec3 global);
   
    void Load(SaveTag tag);
    SaveTag Save(string name);
}
