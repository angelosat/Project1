using Project1.Core.Legacy.Crafting.Defs;
using Project1.Core.Skills;
using Project1.Core.Towns;
using Project1.Framework;

namespace Project1.Core.Materials
{
    [EnsureStaticCtorCall]
    static class MaterialTypeDefOf
    {
        static public readonly MaterialTypeDef Soil = new("Soil", MaterialChemistryDefOf.Inorganic) { JobToExtract = JobDefOf.Digger };
        static public readonly MaterialTypeDef Stone = new("Stone", MaterialChemistryDefOf.Inorganic) { JobToExtract = JobDefOf.Miner };
        static public readonly MaterialTypeDef Metal = new("Metal", MaterialChemistryDefOf.Inorganic) { JobToExtract = JobDefOf.Miner, SkillToRefine = SkillDefOf.Smithing };
        static public readonly MaterialTypeDef Gas = new("Gas", MaterialChemistryDefOf.Inorganic);
        static public readonly MaterialTypeDef Water = new("Water", MaterialChemistryDefOf.Inorganic);
        static public readonly MaterialTypeDef Glass = new("Glass", MaterialChemistryDefOf.Inorganic);

        static public readonly MaterialTypeDef Blood = new("Blood", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Bone = new("Bone", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Skin = new("Skin", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Flesh = new("Meat", MaterialChemistryDefOf.Organic);

        static public readonly MaterialTypeDef Wood = new("Wood", MaterialChemistryDefOf.Organic) { JobToExtract = JobDefOf.Lumberjack, Shininess = .8f };
        static public readonly MaterialTypeDef Seed = new("Seed", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Fruit = new("Fruit", MaterialChemistryDefOf.Organic);
        
        static public readonly MaterialTypeDef Fiber = new("Fiber", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Dye = new("Dye", MaterialChemistryDefOf.Inorganic);

        static public readonly MaterialTypeDef Crystal = new("Crystal", MaterialChemistryDefOf.Inorganic);
        static public readonly MaterialTypeDef FossilFuel = new("FossilFuel", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Sediment = new("Sediment", MaterialChemistryDefOf.Inorganic);

        static MaterialTypeDefOf()
        {
            Def.Register(typeof(MaterialTypeDefOf));
        }
    }
}