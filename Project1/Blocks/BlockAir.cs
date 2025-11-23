namespace Start_a_Town_
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
