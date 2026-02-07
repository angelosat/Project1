using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Simulation;

namespace Project1.Core
{
    public interface IBlockState
    {
        void Apply(MapBase map, Vector3 global);
        void Apply(ref byte blockdata);
        void Apply(Block.Data data);
        //void FromCraftingReagent(GameObject material);
        Color GetTint(byte p);
        string GetName(byte p);
    }
}
