using System.Collections.Generic;

namespace Project1.Core.Crafting
{
    public record IngredientGroup
    {
        internal string Label;
        internal List<IngredientGroupEntry> Entries = [];
    }
}
