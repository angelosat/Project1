using Project1.Core.Entities;

namespace Project1.Core.Entities.Stats
{
    static class StatsHelper
    {
        static public float GetStat(this GameObject parent, StatDef statDef)
        {
            return statDef.CalculateFor(parent);
        }
    }
}
