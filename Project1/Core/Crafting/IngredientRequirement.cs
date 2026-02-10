using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Framework;
using Project1.Core.Entities;
using Project1.Core.Materials;

namespace Project1.Core.Crafting
{
    public record IngredientRequirement(HashSet<MaterialRefinementDef> Refinements, int Quantity, IntVec3 Slot, List<Entity> InSlot)
    {
        public readonly HashSet<MaterialDef> FilteredMaterials = [];
        internal bool Matches(Entity e)
        {
            return e.Def == ItemDefOf.Ingredient && this.Refinements.Contains(e.Profile) && !this.FilteredMaterials.Contains(e.Body.Material);
        }
        internal bool MatchesPartial(Entity e, out int missing)
        {
            if (e.Def == ItemDefOf.Ingredient && this.Refinements.Contains(e.Profile) && !this.FilteredMaterials.Contains(e.Body.Material))
            {
                missing = this.Quantity - e.StackSize;
                return true;
            }
            missing = -1;
            return false;
        }
        public IngredientRequirement ToggleMaterial(MaterialDef mat)
        {
            if (this.FilteredMaterials.Contains(mat))
                this.FilteredMaterials.Remove(mat);
            else
                this.FilteredMaterials.Add(mat);
            return this;
        }
    }
}