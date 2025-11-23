namespace Start_a_Town_
{
    static class MaterialTypeDefOf
    {
        static public readonly MaterialTypeDef Soil = new("Soil", MaterialCategory.Inorganic) { SkillToExtract = JobDefOf.Digger };
        static public readonly MaterialTypeDef Stone = new("Stone", MaterialCategory.Inorganic) { SkillToExtract = JobDefOf.Miner };
        static public readonly MaterialTypeDef Metal = new("Metal", MaterialCategory.Inorganic) { ReactionClass = ReactionClass.Tools, SkillToExtract = JobDefOf.Miner };
        static public readonly MaterialTypeDef Gas = new("Gas", MaterialCategory.Inorganic);
        static public readonly MaterialTypeDef Water = new("Water", MaterialCategory.Inorganic);
        static public readonly MaterialTypeDef Glass = new("Glass", MaterialCategory.Inorganic);
        static public readonly MaterialTypeDef Meat = new("Meat", MaterialCategory.Creature) { ReactionClass = ReactionClass.Protein };
        static public readonly MaterialTypeDef Blood = new("Blood", MaterialCategory.Creature);
        static public readonly MaterialTypeDef Bone = new("Bone", MaterialCategory.Creature);
        static public readonly MaterialTypeDef Fruit = new("Fruit", MaterialCategory.Plant) { ReactionClass = ReactionClass.Protein };
        static public readonly MaterialTypeDef Dye = new("Dye", MaterialCategory.Plant);
        static public readonly MaterialTypeDef Wood = new("Wood", MaterialCategory.Plant) { ReactionClass = ReactionClass.Tools, SkillToExtract = JobDefOf.Lumberjack, Shininess = .8f };
        static public readonly MaterialTypeDef PlantStem = new("PlantStem", MaterialCategory.Plant);
        static public readonly MaterialTypeDef Seed = new("Seed", MaterialCategory.Plant);

        static MaterialTypeDefOf()
        {
            Def.Register(typeof(MaterialTypeDefOf));
        }
    }
}