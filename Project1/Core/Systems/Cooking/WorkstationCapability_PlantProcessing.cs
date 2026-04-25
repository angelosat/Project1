using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Skills;
using Project1.Core.Systems.Crafting;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Plants;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Cooking;

sealed class AddOrderRequest_ExtractSeeds : AddOrderRequest
{
    internal AddOrderRequest_ExtractSeeds() : base(WorkstationCapabilityDefOf.PlantProcessing, null)
    {

    }
    public override string GetLabel()
       => "Extract Seeds";
}
internal class WorkstationCapability_PlantProcessing : WorkstationCapabilityWorker
{
    public override WorkstationCapabilityDef CapabilityDef => WorkstationCapabilityDefOf.PlantProcessing;

    public override bool CreatesUnfinished => false;

    public override SkillDef CraftingSkill => SkillDefOf.Argiculture;

    public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
    {
        yield return new AddOrderRequest_ExtractSeeds();
    }

    public override IEnumerable<BoneDef> GetBoneLayout()
    {
        yield return BoneDefOf.Item;
    }

    public override IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
    {
        yield return new CraftingRule(BoneDefOf.Item, ItemDefOf.Ingredient, [MaterialRefinementDefOf.FruitRaw], [MaterialTypeDefOf.Fruit], 1);
    }
    internal override Entity CreateProduct(Actor actor, CraftingOrder order, IEnumerable<Entity> ingredients, QualityDef quality)
    {
        var fruit = ingredients.First();
        var seeds = PlantSystem.CreateSeeds(fruit.PrimaryMaterial);
        fruit.Consume(1);
        return seeds;
    }
}
