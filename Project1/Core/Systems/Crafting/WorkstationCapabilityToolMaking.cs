using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Skills;
using Project1.Core.Systems.Tools;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Crafting
{
    public class WorkstationCapabilityToolMaking/*(WorkstationCapabilityDef def)*/ : WorkstationCapabilityWorker/*(def)*/
    {
        public override WorkstationCapabilityDef CapabilityDef => WorkstationCapabilityDefOf.ToolMaking;
        public override bool CreatesUnfinished => true;
        public override SkillDef CraftingSkill => SkillDefOf.Crafting;


        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
        {
            return Def.Get<ToolProfileDef>()
                .Select(def => new AddOrderRequest(this.CapabilityDef, def)
                    .AddCondition(new CraftingOrderModuleReq(2)));
        }

        public override IEnumerable<BoneDef> GetBoneLayout()
        {
            foreach (var rule in ToolSystem.GetRules())
                yield return rule.Bone;
        }

        public override IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
        {
            if (recipe is ToolProfileDef tool)
            {
                foreach (var rule in ToolSystem.GetRules())
                    yield return new(rule.Bone, ItemDefOf.Ingredient, [rule.Refinement], [.. rule.Types.Select(mr => mr.MaterialType)], 1);
            }
        }

        public override IEnumerable<(Def[] validRefinements, int quantity)> GetValidIngredientsPerSlot(Def recipe)
        {
            if (recipe is ToolProfileDef tool)
            {
                foreach (var rule in ToolSystem.GetRules())
                    yield return (rule.Types.ToArray(), 1);
            }
        }
    }
}
