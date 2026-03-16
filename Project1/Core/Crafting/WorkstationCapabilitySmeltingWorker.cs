using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Systems.Materials;
using System.Collections.Generic;

namespace Project1.Core.Crafting
{
    public class WorkstationCapabilitySmeltingWorker(WorkstationCapabilityDef def) : WorkstationCapabilityWorker(def)
    {
        public override bool CreatesUnfinished => false;
        public override SkillDef CraftingSkill => SkillDefOf.Smithing;
        public override (ResourceDef resource, int value) ResourceConsumption => (ResourceDefOf.Fuel, 5);

        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
        {
            yield return new AddOrderRequest(this.CapabilityDef, MaterialRefinementDefOf.Ingots);
        }

        public override IEnumerable<BoneDef> GetBoneLayout()
        {
            yield return BoneDefOf.Item;
        }

        public override IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
        {
            if (recipe is MaterialRefinementDef matRefinement)
            {
                yield return new(BoneDefOf.Item, ItemDefOf.Ingredient, [matRefinement.Source], [matRefinement.Source.MaterialType], 1);
            }
        }

        public override IEnumerable<(Def[] validRefinements, int quantity)> GetValidIngredientsPerSlot(Def recipe)
        {
            if(recipe is MaterialRefinementDef matRefinement)
                yield return ([matRefinement.Source], 1);
        }
    }
}
