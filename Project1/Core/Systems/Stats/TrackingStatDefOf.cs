using Project1.Framework;

namespace Project1.Core.Systems.Stats;

[EnsureStaticCtorCall]
public static class TrackingStatDefOf
{
    public static readonly TrackingStatDef TotalHits = new("Total Hits");
    public static readonly TrackingStatDef TotalDamage = new("Total Damage");

    static TrackingStatDefOf()
    {
        Def.Register(typeof(TrackingStatDefOf));
    }
}
