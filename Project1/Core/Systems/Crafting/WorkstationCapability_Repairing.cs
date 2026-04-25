using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Skills;
using System.Collections.Generic;

namespace Project1.Core.Systems.Crafting;


public sealed class WorkstationCapability_Repairing/*(WorkstationCapabilityDef def)*/ : WorkstationCapabilityWorker/*(def)*/
{
    public override WorkstationCapabilityDef CapabilityDef => WorkstationCapabilityDefOf.Repairing;
    public override bool CreatesUnfinished => false;
    public override SkillDef CraftingSkill => SkillDefOf.Crafting;

    public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
    {
        yield return new AddOrderRequest(this.CapabilityDef, null);
    }

    public override IEnumerable<BoneDef> GetBoneLayout()
    {
        yield break;
    }

    public override IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
    {
        yield break;
    }

    //public override IEnumerable<(Def[] validRefinements, int quantity)> GetValidIngredientsPerSlot(Def recipe)
    //{
    //    yield break;
    //}
}
