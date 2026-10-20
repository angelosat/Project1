using Microsoft.Xna.Framework;
using Project1.Framework;

namespace Project1.Core.Entities;

public sealed class QualityDef : Def
{

    public readonly Color Color;
    public readonly float Multiplier;
    public readonly int? Threshold;
    public readonly int Rank;
    public QualityDef(string name, int rank, Color color, float multiplier, int probabilityWeight, int? threshold, int masterySensitivity = 0) : base(name)
    {
        this.Rank = rank;
        this.Color = color;
        this.Multiplier = multiplier;
        this.ProbabilityTableWeight = probabilityWeight;
        this.MasterySensitivity = masterySensitivity;
        this.Threshold = threshold;
    }

    readonly public int ProbabilityTableWeight;
    readonly float MasterySensitivity;

    public float RankEffective => 1 + this.Rank - QualityDefOf.Common.Rank;

    public int GetWeightFromMastery(float masteryRatio)
    {
        var masteryExcess = masteryRatio - 1;
        var mastery = (int)(masteryExcess * this.MasterySensitivity);
        return this.ProbabilityTableWeight + mastery;
    }
}

public sealed class CraftQualityDef : Def
{
    public readonly int Tier;
    public readonly int Threshold;
    public CraftQualityDef(string name, int tier, int threshold) : base(name)
    {
        this.Tier = tier;
        this.Threshold = threshold;
    }
}

[EnsureStaticCtorCall]
static public class CraftQualityDefOf
{
    static public readonly CraftQualityDef Crude = new("Crude", 1, 5);
    static public readonly CraftQualityDef Standard = new("Standard", 2, 40);
    static public readonly CraftQualityDef Fine = new("Fine", 3, 25);
    static public readonly CraftQualityDef Superior = new("Superior", 3, 15);
    static public readonly CraftQualityDef Exceptional = new("Exceptional", 4, 10);
    static public readonly CraftQualityDef Masterwork = new("Masterwork", 5, 4);
    static public readonly CraftQualityDef Peerless = new("Peerless", 6, 1);

    static CraftQualityDefOf()
    {
        Def.Register(typeof(CraftQualityDefOf));
    }
}