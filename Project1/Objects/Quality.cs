using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace Start_a_Town_
{
    public sealed class Quality : Def
    {
        static readonly Random Rand = new();

        public readonly Color Color;
        public readonly float Multiplier;
        public Quality(string name, Color color, float multiplier, int probabilityWeight, int masterySensitivity = 0) : base(name)//$"ItemQuality{label}")
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

        static Quality[] _allCached;
        static Quality[] All => _allCached ??= Def.GetDefs<Quality>().ToArray();

        public static Quality GetRandom(Random rand, float mastery)
        {
            return All.SelectRandomWeighted(rand, q => q.GetWeightFromMastery(mastery));
        }

        public static Quality GetRandom(Random rand)
        {
            return All.SelectRandomWeighted(rand, q => q.ProbabilityTableWeight);
        }

        public static Quality GetRandom()
        {
            return All.SelectRandomWeighted(Rand, q => q.ProbabilityTableWeight);
        }
    }
}
