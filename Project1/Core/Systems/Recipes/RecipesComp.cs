using Project1.Core.Entities;
using Project1.Core.Helpers;
using System.Collections.Generic;

namespace Project1.Core.Systems.Recipes;

class RecipeKnowledge(Def recipe)
{
    Def Recipe = recipe;
    internal int TimesCrafted;
    int StudyUnits;           // persistent progression
    Accumulator StudyAccum;   // produces units over time
}
internal class RecipesComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Recipes;

    public override string Name => "Recipes";

    Dictionary<Def, RecipeKnowledge> _knowledge = [];

    internal void Add(Def profile)
    {
        if (!this._knowledge.TryGetValue(profile, out var entry))
            this._knowledge[profile] = entry = new(profile);
        entry.TimesCrafted++;
    }
}
