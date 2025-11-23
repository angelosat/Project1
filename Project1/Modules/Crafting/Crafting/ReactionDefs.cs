using System;

namespace Start_a_Town_
{
    partial class Reaction
    {
        [Obsolete]
        static public readonly Reaction Repairing = new Reaction("Repair", SkillDefOf.Tinkering)
            .AddBuildSite(IsWorkstation.Types.Workbench)
            .AddIngredient(new Ingredient("item")
                .SetAllow(ItemCategoryDefOf.Equipment, true)
                .AddResourceFilter(ResourceDefOf.Durability)
                .Preserve())
            .AddProduct(new Product("item").RestoreDurability());
    }
}
