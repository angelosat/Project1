using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Plants;
using System.Collections.Generic;

namespace Project1.Core.Crafting;

public sealed class WorkstationCapabilityCooking : WorkstationCapabilityWorker
{
    public override WorkstationCapabilityDef CapabilityDef => WorkstationCapabilityDefOf.Cooking;
    public override bool CreatesUnfinished => false;
    public override SkillDef CraftingSkill => SkillDefOf.Cooking;
    public override (ResourceDef resource, int value) ResourceConsumption => (ResourceDefOf.Fuel, 5);

    public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
    //=> Def.Get<ConsumableDef>().Select(def => new AddOrderRequest(this.CapabilityDef, def));
    {
        yield return new(this.CapabilityDef, ConsumableDefOf.Pie);
    }

    public override IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
    {
        yield return new CraftingRule(BoneDefOf.Item, ItemDefOf.Ingredient, [MaterialRefinementDefOf.FruitRaw, MaterialRefinementDefOf.MeatRaw], [MaterialTypeDefOf.Fruit, MaterialTypeDefOf.Flesh], 1);
    }

    public override IEnumerable<(Def[] validRefinements, int quantity)> GetValidIngredientsPerSlot(Def recipe)
    {
        if (recipe is ConsumableDef cons)
            yield return ([PlantSpeciesDefOf.Berry], 1);
    }

    public override IEnumerable<BoneDef> GetBoneLayout()
    {
        yield return BoneDefOf.Item;
    }

    internal override int GetOutputStackSize(Def recipe)
    {
        return ItemDefOf.Ingredient.StackCapacity;
    }
}
