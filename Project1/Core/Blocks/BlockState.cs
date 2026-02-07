using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Blocks;
using Project1.Core.Simulation;

namespace Project1.Core.Blocks
{
    class BlockState : IBlockState
    {
        public virtual byte Data
        {
            get
            {
                return 0;
            }
        }

        public virtual void FromCraftingReagent(GameObject material)
        {

        }
        public virtual Color GetTint(byte p)
        {
            return Color.White;
        }
        public virtual string GetName(byte p)
        {
            return "";
        }

        public void Apply(MapBase map, Vector3 global)
        {
            map.GetCell(global).BlockData = this.Data;
        }
        public void Apply(ref byte blockdata)
        {
            blockdata = this.Data;
        }
        public void Apply(Block.Data data)
        {
            data.Value = this.Data;
        }
    }
}
