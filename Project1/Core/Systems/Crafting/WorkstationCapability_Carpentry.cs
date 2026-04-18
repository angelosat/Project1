using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Skills;
using Project1.Core.Systems.Materials;
using System.Collections.Generic;

namespace Project1.Core.Systems.Crafting;

public sealed class WorkstationCapability_Carpentry : WorkstationCapabilityWorker
{
    public override WorkstationCapabilityDef CapabilityDef => WorkstationCapabilityDefOf.Carpentry;

    public override bool CreatesUnfinished => false;

    public override SkillDef CraftingSkill => SkillDefOf.Carpentry;
    public override IEnumerable<BoneDef> GetBoneLayout()
    {
        yield return BoneDefOf.Item;
    }
    public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
    {
        yield return new AddOrderRequest(this.CapabilityDef, MaterialRefinementDefOf.Planks);
    }
    
    public override IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
    {
        if (recipe is MaterialRefinementDef matRefinement)
        {
            yield return new(BoneDefOf.Item, ItemDefOf.Ingredient, [matRefinement.Source], [matRefinement.Source.MaterialType], 1);
        }
    }
}
