using Microsoft.Xna.Framework;
using Project1.Core.Base;
using Project1.Core.Helpers.Collections;
using System;
using System.Linq;

namespace Project1.Core.Entities
{
    public sealed class QualityDef : Def
    {
        static readonly Random Rand = new();

        public readonly Color Color;
        public readonly float Multiplier;
        public QualityDef(string name, Color color, float multiplier, int probabilityWeight, int masterySensitivity = 0) : base(name)//$"ItemQuality{label}")
        {
            this.Color = color;
            this.Multiplier = multiplier;
            this.ProbabilityTableWeight = probabilityWeight;
            this.MasterySensitivity = masterySensitivity;
        }

        readonly int ProbabilityTableWeight;
        readonly float MasterySensitivity;
        public int GetWeightFromMastery(float masteryRatio)
        {
            var masteryExcess = masteryRatio - 1;
            var mastery = (int)(masteryExcess * this.MasterySensitivity);
            return this.ProbabilityTableWeight + mastery;
        }

        static QualityDef[] _allCached;
        static QualityDef[] All => _allCached ??= Def.GetDefs<QualityDef>().ToArray();

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
