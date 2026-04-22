using Project1.Core.Helpers;
using Project1.Framework.Events;

namespace Project1.Core.Systems.Recipes;

sealed class RecipeKnowledge(Def recipe)
{
    internal ChangeNotifier Update = new();
    internal Def Recipe { get; init; } = recipe;
    internal int TimesCrafted
    {
        get => field;
        set
        {
            field = value;
            this.Update.Notify();
        }
    }
    int StudyUnits;
    Accumulator StudyAccum;
}
