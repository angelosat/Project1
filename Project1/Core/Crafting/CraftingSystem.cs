using Project1.Core.Entities;
using Project1.Core.Resources;
using Project1.Core.Systems.Materials;

namespace Project1.Core.Crafting
{
    internal class CraftingSystem
    {
        public static bool IsFuel(Entity i)
            => GetFuelValue(i) > 0;

        public static int GetFuelValue(Entity i) 
            => (i.Def == ItemDefOf.Ingredient && i.Profile is MaterialRefinementDef matRefDef ? matRefDef.FuelProduction : 0);

        public record struct ResourceYield(ResourceDef Resource, int Yield) { }
    }
}
