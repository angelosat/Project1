namespace Start_a_Town_
{
    class BlockStone : Block
    {
        public override bool IsMinable => true;

        public BlockStone()
            : base("Cobblestone", 0, 1, true, true)
        {
            this.BreakProduct = RawMaterialDefOf.Boulders;
            this.LoadVariations("stone5height19");
            this.Ingredient = new Ingredient()//RawMaterialDef.Boulders, MaterialDefOf.Stone, null, 1);
                .SetAllow(RawMaterialDefOf.Boulders, true)
                .SetAllow(MaterialDefOf.Stone, true);
            this.BuildProperties.Category = ConstructionCategoryDefOf.Walls;
        }
        public override Particles.ParticleEmitterSphere GetEmitter()
        {
            return base.GetDustEmitter();
        }
        //public override MaterialDef GetMaterial(byte data)
        //{
        //    return MaterialDefOf.Stone;
        //}
    }
}
