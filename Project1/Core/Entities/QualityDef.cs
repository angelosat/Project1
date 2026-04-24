using System;
using Microsoft.Xna.Framework;
using Project1.Framework.Helpers;

namespace Project1.Core.Entities
{
    public sealed class QualityDef : Def
    {
        static readonly Random Rand = new();

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
        public int GetWeightFromMastery(float masteryRatio)
        {
            var masteryExcess = masteryRatio - 1;
            var mastery = (int)(masteryExcess * this.MasterySensitivity);
            return this.ProbabilityTableWeight + mastery;
        }

        static QualityDef[] _allCached;
        static QualityDef[] All => _allCached ??= [.. Get<QualityDef>()];

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
    }
}
