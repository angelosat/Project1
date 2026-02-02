using Project1.Framework.Blocks;

namespace Start_a_Town_
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
