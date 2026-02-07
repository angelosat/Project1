using Project1.Core.Blocks;

namespace Project1.Core
{
    class BlockAir : Block
    {
        public BlockAir()
            : base("Air", 1, 0, false, false)
        {
            this.HidingAdjacent = false;
        }
    }
}
