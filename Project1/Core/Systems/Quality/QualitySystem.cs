using Project1.Core.Entities;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Systems.Quality;

internal static class QualitySystem
{
    static readonly Random Rand = new();

    static QualityDef[] All => field ??= [.. Def.Get<QualityDef>()];
    static CraftsmanshipDef[] AllCraftQualities => field ??= [.. Def.Get<CraftsmanshipDef>()];

    public static QualityDef Random
        => All.SelectRandomWeighted(Rand, q => q.ProbabilityTableWeight);
    
    public static QualityDef GetRandom(Random rand, float mastery)
    {
        return All.SelectRandomWeighted(rand, q => q.GetWeightFromMastery(mastery));
    }
    public static CraftsmanshipDef GetRandomCraftsmahship(Random rand)
        => AllCraftQualities.SelectRandomWeighted(rand, q => q.Threshold);

    public static QualityDef GetRandom(Random rand)
    {
        return All.SelectRandomWeighted(rand, q => q.ProbabilityTableWeight);
    }

    public static QualityDef GetRandom()
    {
        return All.SelectRandomWeighted(Rand, q => q.ProbabilityTableWeight);
    }

    public static void Randomize(Entity item)
    {
        item.SetQuality(Random);
        //var craftQuality = AllCraftQualities.SelectRandomWeighted(Rand, c => c.Threshold);
        //item.Craftsmanship = craftQuality;
        //item.SetName($"{craftQuality.LabelReadable} {item.Name}");
    }
}
