using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    internal class CraftingSystem
    {
        static public IEnumerable<Def> GetCraftables(WorkstationCapabilityDef craftableDef)
        {
            var specific = craftableDef.ProfileCategory;
            var defs = Def.GetDefs(specific);
            if (craftableDef.SpecificRecipes.Any())
                defs = defs.Intersect(craftableDef.SpecificRecipes);
            return defs;
        }
        static public SkillDef GetCraftingSkill(Def recipe)
        {
            return recipe switch
            {
                MaterialRefinementDef => ((MaterialRefinementDef)recipe).MaterialType.SkillToRefine,
                ToolProfileDef => SkillDefOf.Crafting,
                _ => throw new ArgumentException("Def was not of a craftable item", nameof(recipe))
            };
        }
        static public IEnumerable<(BoneDef bone, MaterialRefinementDef[] validRefinements, int quantity)> GetCraftingRules(Def recipe)
        {
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
            if (recipe is MaterialRefinementDef matRefinement)
            {
                yield return new(BoneDefOf.Item, [matRefinement.Source], 1);
            }
            else if (recipe is ToolProfileDef tool)
            {
                foreach (var rule in ToolSystem.GetRules())
                    yield return new(rule.Bone, [.. rule.Types], 1);
            }
            else
                throw new ArgumentException("Def was not of a craftable item", nameof(recipe));
        }
        static public IEnumerable<(MaterialRefinementDef[] validRefinements, int quantity)> GetValidIngredientsPerSlot(Def recipe)
        {
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
            if (recipe is MaterialRefinementDef matRefinement)
            {
                yield return BoneDefOf.Item;
            }
            else if (recipe is ToolProfileDef tool)
            {
                foreach (var rule in ToolSystem.GetRules())
                    yield return (rule.Bone);
            }
            else
                throw new ArgumentException("Def was not of a craftable item", nameof(recipe));

        }
        public static bool IsFuel(Entity i)
        {
            return GetFuelValue(i) > 0;
            //return i.Def == ItemDefOf.Ingredient &&
            //                i.Profile is MaterialRefinementDef matRefDef &&
            //                matRefDef.FuelProduction > 0;
        }
        public static int GetFuelValue(Entity i) => i.Def == ItemDefOf.Ingredient && i.Profile is MaterialRefinementDef matRefDef ? matRefDef.FuelProduction : 0;

    }
    public record struct CraftingRule(BoneDef Bone, HashSet<MaterialRefinementDef> Forms, int Quantity)
    {
        public readonly bool Matches(Entity item, out int missingAmount)
        {
            if (item.Def == ItemDefOf.Ingredient && this.Forms.Contains(item.Profile))
            {
                missingAmount = Quantity - item.StackSize;
                return true;
            }
            missingAmount = -1;
            return false;
        }
    }
}
