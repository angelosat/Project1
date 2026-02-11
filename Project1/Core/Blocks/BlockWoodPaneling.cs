namespace Project1.Core.Blocks
{
    class BlockWoodPaneling : Block
    {
        public BlockWoodPaneling()
            : base("WoodPaneling", 0, 1, true, true)
        {
            this.LoadVariations("woodvertical");
        }
    }
}
