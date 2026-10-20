using Project1.Core.Entities;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Systems.Quality;

internal static class QualitySystem
{
    static readonly Random Rand = new();

    static QualityDef[] All => field ??= [.. Def.Get<QualityDef>()];
    static CraftQualityDef[] AllCraftQualities => field ??= [.. Def.Get<CraftQualityDef>()];

    public static QualityDef Random
        => All.SelectRandomWeighted(Rand, q => q.ProbabilityTableWeight);
    
    public static QualityDef GetRandom(Random rand, float mastery)
    {
        return All.SelectRandomWeighted(rand, q => q.GetWeightFromMastery(mastery));
    }

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
        var craftQuality = AllCraftQualities.SelectRandomWeighted(Rand, c => c.Threshold);
        item.CraftQuality = craftQuality;
        //item.SetName($"{craftQuality.LabelReadable} {item.Name}");
    }
}
