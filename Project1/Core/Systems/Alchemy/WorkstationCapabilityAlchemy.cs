using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Crafting;
using Project1.Core.Skills;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Alchemy;

internal class WorkstationCapabilityAlchemy : WorkstationCapabilityWorker
{
    public override WorkstationCapabilityDef CapabilityDef => WorkstationCapabilityDefOf.Alchemy;

    public override bool CreatesUnfinished => false;

    public override SkillDef CraftingSkill => SkillDefOf.Alchemy;

    public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
    {
        return PotionSystem.Recipes.Select(key => new AddOrderRequest_Alchemy(this.CapabilityDef, key.effect, key.target));
        //foreach (var key in PotionSystem.Recipes)
        //    yield return new AddOrderRequest_Alchemy(this.CapabilityDef, key.effect, key.target);
    }

    public override IEnumerable<BoneDef> GetBoneLayout()
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<(Def[] validRefinements, int quantity)> GetValidIngredientsPerSlot(Def recipe)
    {
        throw new NotImplementedException();
    }
}
