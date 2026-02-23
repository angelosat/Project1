using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Legacy.Storage;
using System.Collections.Generic;

namespace Project1.Core.Legacy.Properties
{
    public class CraftingProperties
    {
        public Dictionary<BoneDef, Reaction.Reagent> Reagents = [];
        public List<Ingredient> Ingredients = [];
        public bool IsBuildingMaterial;
        public bool IsCraftingMaterial;
        readonly List<ItemCategory> MadeFrom = [];

        public CraftingProperties MakeableFrom(params ItemCategory[] cat)
        {
            this.MadeFrom.AddRange(cat);
            return this;
        }
        public bool CanBeMadeFrom(ItemDef def)
        {
            return this.MadeFrom.Contains(def.Category);
        }
        public CraftingProperties AddIngredient(Ingredient ing)
        {
            this.Ingredients.Add(ing);
            return this;
        }

        public static readonly CraftingProperties ToolCraftingProperties = new()
        {
            Reagents = new Dictionary<BoneDef, Reaction.Reagent>()
                {
                    { BoneDefOf.ToolHandle, new Reaction.Reagent("Handle", new Ingredient(null, null, null).SetAllowed(ItemCategoryDefOf.Manufactured, true)) }, //.IsBuildingMaterial()
                    { BoneDefOf.ToolHead, new Reaction.Reagent("Head", new Ingredient(null, null, null).SetAllowed(ItemCategoryDefOf.Manufactured, true))  }
                }
        };
    }
}
