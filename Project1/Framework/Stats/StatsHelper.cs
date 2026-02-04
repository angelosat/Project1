using Project1.Framework.Entities;

namespace Project1.Framework.Stats
{
    static class StatsHelper
    {
        static public float GetStat(this GameObject parent, StatDef statDef)
        {
            return statDef.CalculateFor(parent);
        }
    }
}
