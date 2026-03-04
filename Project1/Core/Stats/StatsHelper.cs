namespace Project1.Core.Entities.Stats
{
    static class StatsHelper
    {
        static public float GetStat(this Entity parent, StatDef statDef)
        {
            return statDef.CalculateFor(parent);
        }
    }
}
