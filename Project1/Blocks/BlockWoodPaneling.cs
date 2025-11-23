namespace Start_a_Town_
{
    class BlockWoodPaneling : Block
    {
        //public override MaterialDef GetMaterial(byte blockdata)
        //{
        //    return MaterialDefOf.LightWood;
        //}
        public BlockWoodPaneling()
            : base("WoodPaneling", 0, 1, true, true)
        {
            this.LoadVariations("woodvertical");
        }
    }
}
