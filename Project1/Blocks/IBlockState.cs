using Microsoft.Xna.Framework;

namespace Start_a_Town_
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
