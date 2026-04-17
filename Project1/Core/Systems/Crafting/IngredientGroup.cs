using System.Collections.Generic;

namespace Project1.Core.Systems.Crafting;

public record IngredientGroup
{
    internal string Label;
    internal List<IngredientGroupEntry> Entries = [];
}
