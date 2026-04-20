using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Skills;
using Project1.Core.Systems.Crafting;
using Project1.Core.Systems.Magic;
using Project1.Core.Systems.Materials;
using System.Collections.Generic;

namespace Project1.Core.Systems.Consumables.Scrolls;

public sealed class WorkstationCapability_Scribing : WorkstationCapabilityWorker
{
    public override WorkstationCapabilityDef CapabilityDef => WorkstationCapabilityDefOf.Scribing;
    public override bool CreatesUnfinished => false;
    public override SkillDef CraftingSkill => SkillDefOf.Scribing;

    public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
    {
        yield return new AddOrderRequest_Scribing(SpellDefOf.Teleporting);
    }

    public override IEnumerable<BoneDef> GetBoneLayout()
    {
        yield return BoneDefOf.Item;
    }

    public override IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
    {
        yield return new CraftingRule(BoneDefOf.Item, ItemDefOf.Ingredient, [MaterialRefinementDefOf.Parchment], [MaterialTypeDefOf.Fiber], 1);
    }

    internal override void PostProcess(Entity product, Actor author, AddOrderRequest parameters)
    {
        var typed = (AddOrderRequest_Scribing)parameters;
        var comp = product.Consumable;
        comp.Spell = typed.Spell;
    }
}
