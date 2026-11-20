using Microsoft.Xna.Framework;

namespace Project1.Core.Entities;

public sealed class QualityDef(string name,
                               int rank,
                               Color color,
                               float multiplier,
                               int probabilityWeight,
                               int? threshold,
                               int masterySensitivity = 0) : Def(name)
{

    public readonly Color Color = color;
    public readonly float Multiplier = multiplier;
    public readonly int? Threshold = threshold;
    public readonly int Rank = rank;
    readonly public int ProbabilityTableWeight = probabilityWeight;
    readonly float MasterySensitivity = masterySensitivity;

    public float RankEffective => 1 + this.Rank - QualityDefOf.Common.Rank;

    public int GetWeightFromMastery(float masteryRatio)
    {
        var masteryExcess = masteryRatio - 1;
        var mastery = (int)(masteryExcess * this.MasterySensitivity);
        return this.ProbabilityTableWeight + mastery;
    }
}
