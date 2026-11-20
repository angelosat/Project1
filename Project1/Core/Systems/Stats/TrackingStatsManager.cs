using System.Collections.Generic;

namespace Project1.Core.Systems.Stats;

internal sealed class TrackingStatsManager
{
    readonly Dictionary<TrackingStatDef, TrackingStat> TrackingStats = [];

    internal TrackingStat GetOrCreateStat(TrackingStatDef stat, float initialValue = 0)
    {
        if (!TrackingStats.TryGetValue(stat, out var runtime))
            TrackingStats.Add(stat, runtime = new(stat, initialValue));
        return runtime;
    }

    internal IEnumerable<TrackingStat> Values => TrackingStats.Values;
}
