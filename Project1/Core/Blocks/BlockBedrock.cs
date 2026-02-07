using Project1.Core.Blocks;

namespace Project1.Core
{
    class BlockBedrock : Block
    {
        public BlockBedrock()
            : base("Stone")
        {
            this.LoadVariations("smoothstone");
        }
    }
}
