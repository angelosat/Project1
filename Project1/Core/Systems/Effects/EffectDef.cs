using Project1.Core.Systems.Magic;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Effects;

public sealed class EffectDef(string name,
                              string verb,
                              EntityEffectWorker worker,
                              Type targetDefType,
                              int? baseDuration,
                              Def context = null,
                              int baseMagnitude = 0,
                              int tickCount = 0
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
              
    internal readonly int? BaseDuration = baseDuration;
              
    internal readonly int BaseMagnitude = baseMagnitude;

    internal readonly int TickCount = tickCount;

    //internal readonly PeriodicTiming? TickPeriod = tickPeriod;
}

//public readonly record struct PeriodicTiming(
//    int Period,
//    int TickCount)
//{
//    public int Duration => Period * TickCount;
//}
