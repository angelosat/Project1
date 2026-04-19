using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Skills;
using Project1.Core.Systems.Crafting;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Materials;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Alchemy;

internal class WorkstationCapability_Alchemy : WorkstationCapabilityWorker
{
    public override Type OrderRequestType => typeof(AddOrderRequest_Alchemy);
    public override WorkstationCapabilityDef CapabilityDef => WorkstationCapabilityDefOf.Alchemy;

    public override bool CreatesUnfinished => false;

    public override SkillDef CraftingSkill => SkillDefOf.Alchemy;

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
    internal override void PostProcess(Entity craftingProduct, Actor author, AddOrderRequest parameters)
    {
        var typedParams = (AddOrderRequest_Alchemy)parameters;
        var eff = typedParams.Effect;
        var tar = typedParams.Target;
        var wrapper = new EntityEffectWrapper(eff, tar, 100, 0); // how determine runtime effect params?
        var comp = craftingProduct.GetComponent<ConsumableComp>();
        comp.Add(wrapper);
    }
    //public override AddOrderRequest DeserializeOrder(IDataReader r)
    //    => AddOrderRequest.Create(r);

    internal override int GetOutputStackSize(Def recipe)
     => ItemDefOf.Consumable.StackCapacity;
}
