namespace Project1.Core.Blocks
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
