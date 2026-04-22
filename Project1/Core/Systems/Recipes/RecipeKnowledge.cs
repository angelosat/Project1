using Project1.Core.Helpers;
using Project1.Framework.Events;
using Project1.Framework.Interfaces;
using System;

namespace Project1.Core.Systems.Recipes;

sealed class RecipeKnowledge(Def recipe) : IProgressBar
{
    internal ChangeNotifier Update = new();
    internal Def Recipe { get; init; } = recipe;
    int k = 2;
    internal int Level => (int)Math.Floor(Math.Sqrt(this.TimesCrafted / k));
    int CurrentThreshold => (this.Level * this.Level) * k;
    int NextThreshold => ((this.Level + 1) * (this.Level + 1)) * k;
    float Progress => this.CurrentXp / (float)this.NextXp;

    internal int CurrentXp =>  this.TimesCrafted - this.CurrentThreshold;
    internal int NextXp => this.NextThreshold - this.CurrentThreshold;
    internal int TimesCrafted
    {
        get => field;
        set
        {
            field = value;
            this.Update.Notify();
        }
    }

    public float Percentage => this.Progress;

    int StudyUnits;
    Accumulator StudyAccum;
}
