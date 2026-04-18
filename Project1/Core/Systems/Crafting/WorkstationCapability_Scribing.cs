using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Skills;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Materials;
using System.Collections.Generic;

namespace Project1.Core.Systems.Crafting;

public sealed class WorkstationCapability_Scribing : WorkstationCapabilityWorker
{
    public override WorkstationCapabilityDef CapabilityDef => WorkstationCapabilityDefOf.Scribing;
    public override bool CreatesUnfinished => false;
    public override SkillDef CraftingSkill => SkillDefOf.Scribing;

    public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
    {
        yield return new(this.CapabilityDef, ConsumableDefOf.TownScroll);
    }

    public override IEnumerable<BoneDef> GetBoneLayout()
    {
        yield return BoneDefOf.Item;
    }

    public override IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
    {
        yield return new CraftingRule(BoneDefOf.Item, ItemDefOf.Ingredient, [MaterialRefinementDefOf.Parchment], [MaterialTypeDefOf.Fiber], 1);
    }
}
