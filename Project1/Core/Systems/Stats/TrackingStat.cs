using Project1.Core.Entities;
using Project1.Framework;
using Project1.Framework.Events;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Stats;

public sealed class TrackingStatDef(string name) : Def(name)
{

}

[EnsureStaticCtorCall]
public static class TrackingStatDefOf
{
    public static readonly TrackingStatDef TotalHits = new("Total Hits");
    public static readonly TrackingStatDef TotalPreventedDamage = new("Total Prevented Damage");

    static TrackingStatDefOf()
    {
        Def.Register(typeof(TrackingStatDefOf));
    }
}

internal sealed class TrackingStat(TrackingStatDef def, float initialValue = 0) : IUpdatable
{
    internal readonly TrackingStatDef Def = def;
    internal readonly ChangeNotifier Notifier = new();
    internal float Value
    {
        get;
        set
        {
            var oldValue = value;
            field = value;
            if (oldValue != value)
                this.Notifier.Notify();
        }
    } = initialValue;

    public IDisposable Subscribe(Action handler)
        => this.Notifier.Subscribe(handler);
}

internal sealed class TrackingStatsManager
{
    readonly Dictionary<TrackingStatDef, TrackingStat> TrackingStats = [];

    internal TrackingStat GetOrCreateStat(TrackingStatDef stat, float initialValue = 0)
    {
        if (!this.TrackingStats.TryGetValue(stat, out var runtime))
            this.TrackingStats.Add(stat, runtime = new(stat, initialValue));
        return runtime;
    }

    internal IEnumerable<TrackingStat> Values => this.TrackingStats.Values;
}
