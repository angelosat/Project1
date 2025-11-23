using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Particles
{
    class ParticleDensityLevel
    {
        public string Name;
        public float Factor;
        static readonly List<ParticleDensityLevel> _All = new();

        ParticleDensityLevel(string name, float factor)
        {
            this.Name = name;
            this.Factor = factor;
            _All.Add(this);
        }
        static public readonly ParticleDensityLevel None = new("None", 0);
        static public readonly ParticleDensityLevel Low = new("Low", .25f);
        static public readonly ParticleDensityLevel Medium = new("Medium", .5f);
        static public readonly ParticleDensityLevel High = new("High", 1);

        static public ParticleDensityLevel Current = High;

        public static List<ParticleDensityLevel> All { get { return _All.ToList(); } }
    }
}
