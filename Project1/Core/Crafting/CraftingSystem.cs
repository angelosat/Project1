using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Plants;
using Project1.Core.Systems.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Crafting
{
    internal class CraftingSystem
    {
        //static public IEnumerable<Def> GetCraftables(WorkstationCapabilityDef craftableDef)
        //{
        //    var specific = craftableDef.Output;
        //    var defs = Def.GetDefs(specific);
        //    if (craftableDef.OutputSpecific.Length != 0)
        //        defs = defs.Intersect(craftableDef.OutputSpecific);
        //    return defs;
        //}
        static public SkillDef GetCraftingSkill(Def recipe)
        {
            return recipe switch
            {
                MaterialRefinementDef => ((MaterialRefinementDef)recipe).MaterialType.SkillToRefine,
                ToolProfileDef => SkillDefOf.Crafting,
                ConsumableDef => SkillDefOf.Cooking,
                _ => throw new ArgumentException("Def was not of a craftable item", nameof(recipe))
            };
        }
        //static public IEnumerable<(BoneDef bone, MaterialRefinementDef[] validRefinements, int quantity)> GetCraftingRules(Def recipe)
        //{
        //    //if(recipe is ConsumableDef consumable)
        //    //{
        //    //    yield return (BoneDefOf.Item, [matRefinement.Source], 1);
        //    //}
        //    //else
        //        if (recipe is MaterialRefinementDef matRefinement)
        //    {
        //        yield return (BoneDefOf.Item, [matRefinement.Source], 1);
        //    }
        //    else if (recipe is ToolProfileDef tool)
        //    {
        //        foreach (var rule in ToolSystem.GetRules())
        //            yield return (rule.Bone, rule.Types.ToArray(), 1);
        //    }
        //    else
        //        throw new ArgumentException("Def was not of a craftable item", nameof(recipe));
        //}
        static public IEnumerable<(BoneDef bone, Def[] validRefinements, int quantity)> GetCraftingRules(Def recipe)
        {
            if (recipe is ConsumableDef consumable)
            {
                yield return (BoneDefOf.Item, [PlantSpeciesDefOf.Berry], 1);
            }
            else
                if (recipe is MaterialRefinementDef matRefinement)
            {
                yield return (BoneDefOf.Item, [matRefinement.Source], 1);
            }
            else if (recipe is ToolProfileDef tool)
            {
                foreach (var rule in ToolSystem.GetRules())
                    yield return (rule.Bone, rule.Types.ToArray(), 1);
            }
            else
                throw new ArgumentException("Def was not of a craftable item", nameof(recipe));
        }
        static public IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
        {
            if (recipe is ConsumableDef cons)
            {
                yield return new CraftingRule(BoneDefOf.Item, ItemDefOf.Consumable, [PlantSpeciesDefOf.Berry, ActorDnaDefOf.Npc], [MaterialTypeDefOf.Fruit, MaterialTypeDefOf.Flesh], 1);
            }
            else
                if (recipe is MaterialRefinementDef matRefinement)
                {
                    yield return new(BoneDefOf.Item, ItemDefOf.Ingredient, [matRefinement], [matRefinement.Source.MaterialType], 1);
                }
                else if (recipe is ToolProfileDef tool)
                {
                    foreach (var rule in ToolSystem.GetRules())
                        yield return new(rule.Bone, ItemDefOf.Ingredient, [rule.Refinement], [..rule.Types.Select(mr=>mr.MaterialType)], 1);
                }
                else
                    throw new ArgumentException("Def was not of a craftable item", nameof(recipe));
        }
        //static public IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
        //{
        //    if (recipe is ConsumableDef cons)
        //    {
        //        yield return new CraftingRule(BoneDefOf.Item, [PlantSpeciesDefOf.Berry], 1);
        //    }
        //    else
        //        if (recipe is MaterialRefinementDef matRefinement)
        //    {
        //        yield return new(BoneDefOf.Item, [matRefinement.Source], 1);
        //    }
        //    else if (recipe is ToolProfileDef tool)
        //    {
        //        foreach (var rule in ToolSystem.GetRules())
        //            yield return new(rule.Bone, [.. rule.Types], 1);
        //    }
        //    else
        //        throw new ArgumentException("Def was not of a craftable item", nameof(recipe));
        //}
        //static public IEnumerable<(MaterialRefinementDef[] validRefinements, int quantity)> GetValidIngredientsPerSlot(Def recipe)
        static public IEnumerable<(Def[] validRefinements, int quantity)> GetValidIngredientsPerSlot(Def recipe)
        {
            if (recipe is ConsumableDef cons)
            {
                yield return ([PlantSpeciesDefOf.Berry], 1);
            }
            else
                if (recipe is MaterialRefinementDef matRefinement)
            {
                yield return ([matRefinement.Source], 1);
            }
            else if (recipe is ToolProfileDef tool)
            {
                foreach (var rule in ToolSystem.GetRules())
                    yield return (rule.Types.ToArray(), 1);
            }
            else
                throw new ArgumentException("Def was not of a craftable item", nameof(recipe));
        }
        static public IEnumerable<BoneDef> GetSlotMapping(Def recipe)
        {
            if (recipe is MaterialRefinementDef)
            {
                yield return BoneDefOf.Item;
            }
            else if (recipe is ToolProfileDef)
            {
                foreach (var rule in ToolSystem.GetRules())
                    yield return rule.Bone;
            }
            else
                throw new ArgumentException("Def was not of a craftable item", nameof(recipe));
        }
        static public Dictionary<BoneDef, Entity> GetIngredientMapping(Def recipe, IEnumerable<Entity> ingredients)
        {
            var targetBones = GetSlotMapping(recipe);
            return targetBones.Zip(ingredients).ToDictionary();
        }
        static public Dictionary<BoneDef, MaterialDef> MapBonesToMaterials(Def recipe, IEnumerable<MaterialDef> materials)
        {
            var targetBones = GetSlotMapping(recipe);
            return targetBones.Zip(materials).ToDictionary();
        }
        public static bool IsFuel(Entity i)
        {
            return GetFuelValue(i) > 0;
            //return i.Def == ItemDefOf.Ingredient &&
            //                i.Profile is MaterialRefinementDef matRefDef &&
            //                matRefDef.FuelProduction > 0;
        }
        public static int GetFuelValue(Entity i) 
            => //i.StackSize * 
            (i.Def == ItemDefOf.Ingredient && i.Profile is MaterialRefinementDef matRefDef ? matRefDef.FuelProduction : 0);
        public record struct ResourceYield(ResourceDef Resource, int Yield) { }
        //public static ResourceYield GetResourceYield(Entity i)
        //    => //i.StackSize * 
        //    new(i.Def == ItemDefOf.Ingredient && i.Profile is MaterialRefinementDef matRefDef ? matRefDef.FuelProduction : 0);
        public static bool CreatesUnfinished(CraftingOrder order)
        {
            var productDef = order.ProductDef;
            return productDef switch
            {
                MaterialRefinementDef => false,
                ToolProfileDef => true,
                _ => throw new UnreachableException()
            };
        }
    }
    
}
