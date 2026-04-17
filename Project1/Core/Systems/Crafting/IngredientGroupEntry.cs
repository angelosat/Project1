using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Crafting;

public record IngredientGroupEntry
{
    internal string Label;
    internal List<IngredientGroupEntry> Children = [];
    internal Action Toggle;
    internal Func<bool> IsAllowed;
}
