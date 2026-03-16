using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Plants;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Crafting
{
    public class WorkstationCapabilityCookingWorker(WorkstationCapabilityDef def) : WorkstationCapabilityWorker(def)
    {
        public override bool CreatesUnfinished => false;
        public override SkillDef CraftingSkill => SkillDefOf.Cooking;
        public override (ResourceDef resource, int value) ResourceConsumption => (ResourceDefOf.Fuel, 5);

        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
            => Def.GetDefs<ConsumableDef>().Select(def => new AddOrderRequest(this.CapabilityDef, def));

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
    }
}
