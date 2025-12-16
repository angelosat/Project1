namespace Start_a_Town_
{
    static class MaterialTypeDefOf
    {
        static public readonly MaterialTypeDef Soil = new("Soil", MaterialChemistryDefOf.Inorganic) { SkillToExtract = JobDefOf.Digger };
        static public readonly MaterialTypeDef Stone = new("Stone", MaterialChemistryDefOf.Inorganic) { SkillToExtract = JobDefOf.Miner };
        static public readonly MaterialTypeDef Metal = new("Metal", MaterialChemistryDefOf.Inorganic) { ReactionClass = ReactionClass.Tools, SkillToExtract = JobDefOf.Miner };
        static public readonly MaterialTypeDef Gas = new("Gas", MaterialChemistryDefOf.Inorganic);
        static public readonly MaterialTypeDef Water = new("Water", MaterialChemistryDefOf.Inorganic);
        static public readonly MaterialTypeDef Glass = new("Glass", MaterialChemistryDefOf.Inorganic);

        static public readonly MaterialTypeDef Blood = new("Blood", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Bone = new("Bone", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Skin = new("Skin", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Flesh = new("Meat", MaterialChemistryDefOf.Organic) { ReactionClass = ReactionClass.Protein };

        static public readonly MaterialTypeDef Wood = new("Wood", MaterialChemistryDefOf.Organic) { ReactionClass = ReactionClass.Tools, SkillToExtract = JobDefOf.Lumberjack, Shininess = .8f };
        static public readonly MaterialTypeDef Seed = new("Seed", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Fruit = new("Fruit", MaterialChemistryDefOf.Organic) { ReactionClass = ReactionClass.Protein };
        
        static public readonly MaterialTypeDef Fiber = new("Fiber", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Dye = new("Dye", MaterialChemistryDefOf.Inorganic);
 

        static MaterialTypeDefOf()
        {
            Def.Register(typeof(MaterialTypeDefOf));
        }
    }
}