using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    internal class CraftingSystem
    { 
        static public IEnumerable<Def> GetCraftables(CraftableDef craftableDef)
        {
            var specific = craftableDef.CraftableDefType;
            var defs = Def.GetDefs(specific);
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
        static public IEnumerable<(MaterialRefinementDef[] validRefinements, int quantity)> GetValidIngredientsPerSlot(Def recipe)
        {
            if (recipe is MaterialRefinementDef matRefinement)
            {
                yield return ([matRefinement.Source], 1);
            }
            else if (recipe is ToolProfileDef tool)
            {
                foreach(var rule in ToolSystem.GetRules())
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
    }
}
