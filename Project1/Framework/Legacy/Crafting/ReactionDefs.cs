using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Legacy.Storage;
using System;

namespace Project1.Core
{
    partial class Reaction
    {
        [Obsolete]
        static public readonly Reaction Repairing = new Reaction("Repair", SkillDefOf.Tinkering)
            .AddBuildSite(IsWorkstation.Types.Workbench)
            .AddIngredient(new Ingredient("item")
                .SetAllowed(ItemCategoryDefOf.Equipment, true)
                .AddResourceFilter(ResourceDefOf.Durability)
                .Preserve())
            .AddProduct(new Product("item").RestoreDurability());
    }
}
