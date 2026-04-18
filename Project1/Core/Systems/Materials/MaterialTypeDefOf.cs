using Project1.Core.Effects;
using Project1.Core.Skills;
using Project1.Core.Towns.Duties;
using Project1.Framework;

namespace Project1.Core.Systems.Materials
{
    [EnsureStaticCtorCall]
    static class MaterialTypeDefOf
    {
        static public readonly MaterialTypeDef Soil = new("Soil", MaterialChemistryDefOf.Inorganic) { 
            JobToExtract = DutyDefOf.Digger, 
            GatheringSkill = SkillDefOf.Digging };
        static public readonly MaterialTypeDef Stone = new("Stone", MaterialChemistryDefOf.Inorganic) { 
            JobToExtract = DutyDefOf.Miner,
            GatheringSkill = SkillDefOf.Mining
        };
        static public readonly MaterialTypeDef Metal = new("Metal", MaterialChemistryDefOf.Inorganic) { 
            JobToExtract = DutyDefOf.Miner,
            GatheringSkill = SkillDefOf.Mining, 
            SkillToRefine = SkillDefOf.Smithing,
            AlchemyEffect = EffectDefOf.FortifyResource
        };

        static public readonly MaterialTypeDef Gas = new("Gas", MaterialChemistryDefOf.Inorganic);
        static public readonly MaterialTypeDef Water = new("Water", MaterialChemistryDefOf.Inorganic);
        static public readonly MaterialTypeDef Glass = new("Glass", MaterialChemistryDefOf.Inorganic);

        static public readonly MaterialTypeDef Blood = new("Blood", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Bone = new("Bone", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Skin = new("Skin", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Flesh = new("Meat", MaterialChemistryDefOf.Organic);

        static public readonly MaterialTypeDef Wood = new("Wood", MaterialChemistryDefOf.Organic) {
            GatheringSkill = SkillDefOf.Plantcutting,
            JobToExtract = DutyDefOf.Lumberjack, 
            Shininess = .8f };
        static public readonly MaterialTypeDef Seed = new("Seed", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Fruit = new("Fruit", MaterialChemistryDefOf.Organic) { AlchemyEffect = EffectDefOf.RestoreResource };
        
        static public readonly MaterialTypeDef Fiber = new("Fiber", MaterialChemistryDefOf.Organic);
        static public readonly MaterialTypeDef Dye = new("Dye", MaterialChemistryDefOf.Inorganic);

        static public readonly MaterialTypeDef Crystal = new("Crystal", MaterialChemistryDefOf.Inorganic)
        { GatheringSkill = SkillDefOf.Mining };
        static public readonly MaterialTypeDef FossilFuel = new("FossilFuel", MaterialChemistryDefOf.Organic)
        { GatheringSkill = SkillDefOf.Mining };
        static public readonly MaterialTypeDef Sediment = new("Sediment", MaterialChemistryDefOf.Inorganic)
        { GatheringSkill = SkillDefOf.Mining };



        static MaterialTypeDefOf()
        {
            Def.Register(typeof(MaterialTypeDefOf));
        }
    }
}