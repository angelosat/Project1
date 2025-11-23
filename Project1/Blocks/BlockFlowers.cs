namespace Start_a_Town_.Blocks
{
    class BlockFlowers : Block
    {
        //public override MaterialDef GetMaterial(byte blockdata)
        //{
        //    return MaterialDefOf.Soil;
        //}
        public BlockFlowers()
            : base("Flowers", solid: false, opaque: false)
        {
            this.Variations.Add(Atlas.Load("blocks/flowers/flowersred", SliceBlockDepthMap, NormalMap));
            this.Variations.Add(Atlas.Load("blocks/flowers/flowersyellow", SliceBlockDepthMap, NormalMap));
            this.Variations.Add(Atlas.Load("blocks/flowers/flowerswhite", SliceBlockDepthMap, NormalMap));
            this.Variations.Add(Atlas.Load("blocks/flowers/flowerspurple", SliceBlockDepthMap, NormalMap));
            this.DefaultMaterial = MaterialDefOf.Soil;
        }
    }
}
