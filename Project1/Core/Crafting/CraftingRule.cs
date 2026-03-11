using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Systems.Materials;
using System.Collections.Generic;

namespace Project1.Core.Crafting
{
    public record struct CraftingRule(BoneDef Bone, ItemDef Def, HashSet<Def> Profiles, HashSet<MaterialTypeDef> MaterialTypes, int Quantity)
    {
        public readonly bool Matches(Entity item, out int missingAmount)
        {
            missingAmount = Quantity - item.StackSize;

            if (item.Def != this.Def)
                return false;

            if (this.Profiles?.Count > 0 && !this.Profiles.Contains(item.Profile))
                return false;

            if (this.MaterialTypes?.Count > 0 && !this.MaterialTypes.Contains(item.PrimaryMaterial.Type))
                return false;

            return true;
        }
    }
    //public record struct CraftingRule(BoneDef Bone, HashSet<MaterialRefinementDef> Forms, int Quantity)
    ////public record struct CraftingRule(BoneDef Bone, HashSet<Def> Forms, int Quantity)
    //{
    //    public readonly bool Matches(Entity item, out int missingAmount)
    //    {
    //        if (item.Def == ItemDefOf.Ingredient && this.Forms.Contains(item.Profile))
    //        {
    //            missingAmount = Quantity - item.StackSize;
    //            return true;
    //        }
    //        missingAmount = -1;
    //        return false;
    //    }
    //}
}
