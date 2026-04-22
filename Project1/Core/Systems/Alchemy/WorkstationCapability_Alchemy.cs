using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Skills;
using Project1.Core.Systems.Crafting;
using Project1.Core.Systems.Materials;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Alchemy;

internal class WorkstationCapability_Alchemy : WorkstationCapabilityWorker
{
    public override WorkstationCapabilityDef CapabilityDef => WorkstationCapabilityDefOf.Alchemy;

    public override bool CreatesUnfinished => false;

    public override SkillDef CraftingSkill => AlchemyDefOf.Skill;// SkillDefOf.Alchemy;

    public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
    {
        return PotionSystem.Recipes.Select(key => new AddOrderRequest_Alchemy(key.effect, key.target));
    }

    public override IEnumerable<BoneDef> GetBoneLayout()
    {
        yield return BoneDefOf.Item;
    }

    public override IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
    {
        yield return new(BoneDefOf.Item, ItemDefOf.Ingredient, [MaterialRefinementDefOf.FruitRaw, MaterialRefinementDefOf.Powder, MaterialRefinementDefOf.Paste], [MaterialTypeDefOf.Fruit, MaterialTypeDefOf.Flesh], 1);
    }
    internal override int GetOutputStackSize(Def recipe)
     => ItemDefOf.Consumable.StackCapacity;
}
