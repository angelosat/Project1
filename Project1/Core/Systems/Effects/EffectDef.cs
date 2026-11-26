using Project1.Core.Simulation;
using Project1.Core.Systems.Magic;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Effects;

public sealed class EffectDef(string name,
                              string verb,
                              EntityEffectWorker worker,
                              Type targetDefType,
                              SimulationDuration? baseDuration,
                              Def context = null,
                              int baseMagnitude = 0,
                              int? tickCount = null,
                              SimulationDuration? tickPeriod = null
    //PeriodicTiming? tickPeriod = null
    )
    : Def(name)
{
    internal Dictionary<Def, float> StatMods = [];

    internal string Verb = verb;

    internal EntityEffectWorker Worker = worker;

    internal readonly Type TargetDefType = targetDefType;

    internal SpellSchoolDef School { get; init; }

    internal readonly Def Context = context;

    internal readonly SimulationDuration? Duration = baseDuration;

    internal readonly int BaseMagnitude = baseMagnitude;

    internal readonly int? TickCount = tickCount;
    internal readonly SimulationDuration? TickInterval = tickPeriod;

    //internal readonly PeriodicTiming? TickPeriod = tickPeriod;

    //public override IEnumerable<string> ConfigErrors()
    //{
    //    foreach (var error in base.ConfigErrors())
    //        yield return error;

    //    if (this.Duration is { Value: < 0 })
    //        yield return "Duration cannot be negative.";

    //    if (this.TickInterval is { Value: <= 0 })
    //        yield return "Tick interval must be greater than zero.";

    //    if (this.TickCount is <= 0)
    //        yield return "Tick count must be greater than zero.";

    //    if (this.TickInterval is null && this.TickCount is not null)
    //        yield return "Tick count requires a tick interval.";

    //    if (this.TickInterval is not null && this.Duration is not null)
    //        yield return "Periodic effects derive their duration from tick interval and tick count.";
    //}

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors())
            yield return error;

        if (this.Duration is { Value: < 0 })
            yield return "Duration cannot be negative.";

        if (this.TickInterval is { Value: <= 0 })
            yield return "Tick interval must be greater than zero.";

        if (this.TickCount is <= 0)
            yield return "Tick count must be greater than zero.";

        if (this.TickCount is not null && this.TickInterval is null)
            yield return "Tick count requires a tick interval.";

        if (this.TickInterval is not null && this.Duration is not null)
            yield return "Periodic effects derive their duration from tick interval and tick count.";

        if (this.TickInterval is { } interval && this.TickCount is > 0)
        {
            long totalDuration = (long)interval.Value * this.TickCount.Value;

            if (totalDuration > int.MaxValue)
                yield return "Periodic effect duration exceeds the maximum supported simulation duration.";
        }
    }
}

//public readonly record struct PeriodicTiming(
//    int Period,
//    int TickCount)
//{
//    public int Duration => Period * TickCount;
//}
