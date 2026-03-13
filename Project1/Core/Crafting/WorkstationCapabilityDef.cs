using Project1.Core.AI;
using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Skills;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Plants;
using Project1.Core.Systems.Tools;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Crafting
{
    public class WorkstationCapabilityDef(string name, Type workerType) : Def(name)
    {
        public Type Output;
        public Def[] OutputSpecific = [];
        public PlanDef Plan;
        public WorkstationCapabilityWorker Worker = ActivatorSafe<WorkstationCapabilityWorker>.CreateInstance(workerType);

    }
    public abstract class WorkstationCapabilityWorker
    {
        public abstract bool CreatesUnfinished { get; }
        public abstract SkillDef CraftingSkill { get; }

        public abstract IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp);
        public abstract IEnumerable<(Def[] validRefinements, int quantity)> GetValidIngredientsPerSlot(Def recipe);
        public abstract IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe);
        public abstract IEnumerable<BoneDef> GetBoneLayout();

        public Dictionary<BoneDef, Entity> GetIngredientMapping(Def recipe, IEnumerable<Entity> ingredients)
            => this.GetBoneLayout().Zip(ingredients).ToDictionary();
        public Dictionary<BoneDef, MaterialDef> MapBonesToMaterials(Def recipe, IEnumerable<MaterialDef> materials)
            => this.GetBoneLayout().Zip(materials).ToDictionary();
    }
    public class WorkstationCapabilitySmeltingWorker : WorkstationCapabilityWorker
    {
        public override bool CreatesUnfinished => false;
        public override SkillDef CraftingSkill => SkillDefOf.Smithing;

        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
        {
            yield return new AddOrderRequest(WorkstationCapabilityDefOf.Smelting, MaterialRefinementDefOf.Ingots);
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
    public class WorkstationCapabilityCarpentryWorker : WorkstationCapabilityWorker
    {
        public override bool CreatesUnfinished => false;

        public override SkillDef CraftingSkill => SkillDefOf.Carpentry;
        public override IEnumerable<BoneDef> GetBoneLayout()
        {
            yield return BoneDefOf.Item;
        }
        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
        {
            yield return new AddOrderRequest(WorkstationCapabilityDefOf.Carpentry, MaterialRefinementDefOf.Planks);
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
            if (recipe is MaterialRefinementDef matRefinement)
                yield return ([matRefinement.Source], 1);
        }
    }
    public class WorkstationCapabilityToolMakingWorker : WorkstationCapabilityWorker
    {
        public override bool CreatesUnfinished => true;
        public override SkillDef CraftingSkill => SkillDefOf.Crafting;


        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
        {
            return Def.GetDefs<ToolProfileDef>().Select(def => new AddOrderRequest(WorkstationCapabilityDefOf.ToolMaking, def));
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
    public class WorkstationCapabilityRepairingWorker : WorkstationCapabilityWorker
    {
        public override bool CreatesUnfinished => false;
        public override SkillDef CraftingSkill => SkillDefOf.Crafting;

        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
        {
            yield return new AddOrderRequest(WorkstationCapabilityDefOf.Repairing, null);
        }

        public override IEnumerable<BoneDef> GetBoneLayout()
        {
            yield break;
        }

        public override IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
        {
            yield break;
        }

        public override IEnumerable<(Def[] validRefinements, int quantity)> GetValidIngredientsPerSlot(Def recipe)
        {
            yield break;
        }
    }
    public class WorkstationCapabilityCookingWorker : WorkstationCapabilityWorker
    {
        public override bool CreatesUnfinished => false;
        public override SkillDef CraftingSkill => SkillDefOf.Cooking;

        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
            => Def.GetDefs<ConsumableDef>().Select(def => new AddOrderRequest(WorkstationCapabilityDefOf.Cooking, def));

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
