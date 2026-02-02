using Start_a_Town_;

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
