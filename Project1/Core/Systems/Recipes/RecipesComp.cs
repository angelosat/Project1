using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Materials;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Recipes;

internal sealed class RecipesComp : EntityComp
{
    internal event Action<(IEnumerable<RecipeKnowledge> added, IEnumerable<RecipeKnowledge> removed)> Updated;
    public override EntityCompDef CompDef => EntityCompDefOf.Recipes;

    public override string Name => "Recipes";

    Dictionary<Def, RecipeKnowledge> _knowledge = [];
    public IEnumerable<RecipeKnowledge> All => this._knowledge.Values;

    public RecipesComp()
    {
        this._knowledge.Add(MaterialRefinementDefOf.Ingots, new(MaterialRefinementDefOf.Ingots) { TimesCrafted = 5 });
    }

    internal void Add(Def profile)
    {
        if (!this._knowledge.TryGetValue(profile, out var entry))
            this._knowledge[profile] = entry = new(profile);
        entry.TimesCrafted++;
        this.Updated?.Invoke(([entry], []));
        this.World.Events.Post(new ActorRecipeMasteryEvent(this.Owner as Actor, entry));
    }

    public int Get(Def recipe)
        => this._knowledge.TryGetValue(recipe, out var entry) ? entry.Level : 0;
}
